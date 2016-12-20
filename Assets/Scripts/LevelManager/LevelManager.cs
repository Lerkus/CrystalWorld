﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{

    public static LevelManager levelManager;
    private ObjectPool mr;

    private IsleAbstract[,] world;
    private List<IsleAbstract> isles;
    private List<ConnectionAbstract> connections;

    public int WorldWidth;
    public int WorldHeight;

    public float IsleDensity;

    public int Fieldwidth;

    public float AddConnPerIsle;

    private List<Isle> islesObjects;
    private List<Connection> connectionObjects;

    private IsleAbstract currentIsle;
    private IsleAbstract startIsle;

    private System.Random rnd;

    public static LevelManager getLevelManager()
    {
        return levelManager;
    }

    void Awake()
    {
        levelManager = this;
        mr = ObjectPool.getObjectPool();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GenerateMap();

            ObjectPool.getObjectPool().getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.UI).GetComponent<UI_Canvas>().ShowMiniMap();
        }
    }

    public void GenerateMap()
    {
        rnd = new System.Random();

        if (islesObjects != null)
        {
            for (int i = 0; i < islesObjects.Count; i++)
            {
                Destroy(islesObjects[i].gameObject);
            }
            islesObjects.Clear();
        }

        if (connectionObjects != null)
        {
            for (int i = 0; i < connectionObjects.Count; i++)
            {
                Destroy(connectionObjects[i].gameObject);
            }
            connectionObjects.Clear();
        }

        // Create World-Array
        world = new IsleAbstract[WorldWidth, WorldHeight];

        int numberFields = WorldWidth * WorldHeight;

        // create list of all fields

        List<Vector2> fields = new List<Vector2>();
        for (int x = 0; x < WorldWidth; x++)
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                fields.Add(new Vector2(x, y));
            }
        }


        // Create abstract isles and give isles individual fields

        IsleDensity = System.Math.Min(IsleDensity, 1);

        int numberOfIslands = (int)Mathf.Floor(numberFields * IsleDensity);  // Warum schickt Floor einen float zurück? Das war früher nicht so!

        isles = new List<IsleAbstract>();

        for (int i = 0; i < numberOfIslands; i++)
        {
            IsleAbstract isle = new IsleAbstract();
            int tmp = rnd.Next(0, fields.Count);
            isle.Index = fields[tmp];
            isles.Add(isle);
            fields.RemoveAt(tmp);

        }


        // set isles in world-array

        for (int i = 0; i < isles.Count; i++)
        {
            int x = (int)isles[i].Index.x;
            int y = (int)isles[i].Index.y;
            world[x, y] = isles[i];
        }

        buildMinimalTree();

        insertAdditionalConnections();

        inserObjectsOnMap();

        // render world

        renderWorld();

    }

    private void renderWorld()
    {
        islesObjects = new List<Isle>();
        connectionObjects = new List<Connection>();

        Fieldwidth = System.Math.Max(Fieldwidth, 10);

        IsleAbstract isle = null;
        for (int x = 0; x < world.GetLength(0); x++)
        {

            for (int y = 0; y < world.GetLength(1); y++)
            {
                if (world[x, y] == null)
                {
                    continue;
                }

                isle = world[x, y];

                int offset = 0;
                if (x % 2 == 1)
                {
                    offset = Fieldwidth / 2;
                }

                int isleHeight = rnd.Next(-50, 51);
                //int isleHeight = 0;

                Vector3 pos = new Vector3(isle.Index.x * Fieldwidth, isleHeight, (isle.Index.y * Fieldwidth) + offset);
                //Isle isleObj = Instantiate(IslePrefab, pos, new Quaternion()) as Isle;   // TODO REMOVE LINE!
                Isle isleObj = mr.getObject(ObjectPool.categorie.islands, (int)ObjectPool.islands.normal).GetComponent<Isle>();
                isleObj.transform.position = pos;
                isleObj.Initialize(isle);
                isle.IsleObj = isleObj;


                if (isle.Connected == true)
                {
                    //isleObj.gameObject.GetComponent<Renderer>().material.color = new Color(255, 0, 0);   // TODO
                }

                islesObjects.Add(isleObj);
            }
        }

        // render connections
        for (int i = 0; i < connections.Count; i++)
        {
            Connection connectionObj = mr.getObject(ObjectPool.categorie.structures, (int)ObjectPool.structures.connection).GetComponent<Connection>();
            connectionObj.connectionAbstract = connections[i];
            connections[i].connectionObj = connectionObj;

            LineRenderer lineRenderer = connectionObj.GetComponent<LineRenderer>();
            lineRenderer.SetVertexCount(2);
            lineRenderer.SetPosition(0, connections[i].Portal1.portalObj.transform.position + new Vector3(0, 1,0));
            lineRenderer.SetPosition(1, connections[i].Portal2.portalObj.transform.position + new Vector3(0, 1, 0));

            connectionObjects.Add(connectionObj);
        }
    }

    private void buildMinimalTree()
    {
        if (isles == null) return;

        connections = new List<ConnectionAbstract>();

        int tmp = rnd.Next(0, isles.Count);
        IsleAbstract startIsle = isles[tmp];

        searchNeighbour(startIsle, null, -1);
    }

    private void searchNeighbour(IsleAbstract current, IsleAbstract last, int directionFrom)
    {

        if (current.Connected == true)
        {
            return;
        }

        // Connect isles
        current.Connected = true;
        if (last != null)
        {
            connectIsles(last, current, directionFrom);
        }


        List<int> directions = new List<int>();

        for (int i = 0; i < 6; i++)
        {
            directions.Add(i);
        }

        // remove direction which is comes from
        int directionsToCheck = 6;
        if (directionFrom != -1)
        {
            directions.Remove(directionFrom);
            directionsToCheck--;
        }

        for (int i = 0; i < directionsToCheck; i++)
        {
            int tmp = rnd.Next(0, directions.Count);
            int direction = directions[tmp];
            directions.RemoveAt(tmp);

            IsleAbstract isle = null;

            switch (direction)
            {
                case 0:
                    isle = travelUp((int)current.Index.x, (int)current.Index.y);
                    break;
                case 1:
                    isle = travelUpRight((int)current.Index.x, (int)current.Index.y);
                    break;
                case 2:
                    isle = travelDownRight((int)current.Index.x, (int)current.Index.y);
                    break;
                case 3:
                    isle = travelDown((int)current.Index.x, (int)current.Index.y);
                    break;
                case 4:
                    isle = travelDownLeft((int)current.Index.x, (int)current.Index.y);
                    break;
                case 5:
                    isle = travelUpLeft((int)current.Index.x, (int)current.Index.y);
                    break;
            }

            if (isle != null)
            {
                direction = (direction + 3) % 6;
                searchNeighbour(isle, current, direction);

            }
        }

    }

    private void connectIsles(IsleAbstract isle1, IsleAbstract isle2, int directionFrom)
    {
        PortalAbstract portal1 = new PortalAbstract();
        PortalAbstract portal2 = new PortalAbstract();
        ConnectionAbstract connection = new ConnectionAbstract();

        portal1.isleAbstract = isle1;
        portal1.ConnectecPortal = portal2;
        portal1.Connection = connection;
        
        portal2.isleAbstract = isle2;
        portal2.ConnectecPortal = portal1;
        portal2.Connection = connection;
        
        connection.Portal1 = portal1;
        connection.Portal2 = portal2;
        connections.Add(connection);

        switch (directionFrom)
        {
            case 0:
                isle1.PortalDown = portal1;
                isle2.PortalUp = portal2;
                break;
            case 1:
                isle1.PortalDownLeft = portal1;
                isle2.PortalUpRight = portal2;
                break;
            case 2:
                isle1.PortalUpLeft = portal1;
                isle2.PortalDownRight = portal2;
                break;
            case 3:
                isle1.PortalUp = portal1;
                isle2.PortalDown = portal2;
                break;
            case 4:
                isle1.PortalUpRight = portal1;
                isle2.PortalDownLeft = portal2;
                break;
            case 5:
                isle1.PortalDownRight = portal1;
                isle2.PortalUpLeft = portal2;
                break;
        }
    }

    public IsleAbstract travelUp(int startX, int startY)
    {
        IsleAbstract endIsle = null;

        for (int y = startY + 1; y < world.GetLength(1); y++)
        {
            if (world[startX, y] != null)
            {
                endIsle = world[startX, y];
                break;
            }
        }

        return endIsle;
    }

    public IsleAbstract travelDown(int startX, int startY)
    {
        IsleAbstract endIsle = null;

        for (int y = startY - 1; y >= 0; y--)
        {
            if (world[startX, y] != null)
            {
                endIsle = world[startX, y];
                break;
            }
        }

        return endIsle;
    }

    public IsleAbstract travelUpRight(int startX, int startY)
    {
        IsleAbstract endIsle = null;

        int x = startX;
        int y = startY;

        while (true)
        {

            if (x % 2 == 1)
            {
                y++;
            }

            x++;

            if (x >= world.GetLength(0) || y >= world.GetLength(1))
            {
                return null;
            }

            if (world[x, y] != null)
            {
                endIsle = world[x, y];
                break;
            }

        }

        return endIsle;
    }

    public IsleAbstract travelDownRight(int startX, int startY)
    {
        IsleAbstract endIsle = null;

        int x = startX;
        int y = startY;

        while (true)
        {

            if (x % 2 == 0)
            {
                y--;
            }

            x++;

            if (x >= world.GetLength(0) || y < 0)
            {
                return null;
            }

            if (world[x, y] != null)
            {
                endIsle = world[x, y];
                break;
            }

        }

        return endIsle;
    }

    public IsleAbstract travelUpLeft(int startX, int startY)
    {
        IsleAbstract endIsle = null;

        int x = startX;
        int y = startY;

        while (true)
        {

            if (x % 2 == 1)
            {
                y++;
            }

            x--;

            if (x < 0 || y >= world.GetLength(1))
            {
                return null;
            }

            if (world[x, y] != null)
            {
                endIsle = world[x, y];
                break;
            }

        }

        return endIsle;
    }

    public IsleAbstract travelDownLeft(int startX, int startY)
    {
        IsleAbstract endIsle = null;

        int x = startX;
        int y = startY;

        while (true)
        {

            if (x % 2 == 0)
            {
                y--;
            }

            x--;

            if (x < 0 || y < 0)
            {
                return null;
            }

            if (world[x, y] != null)
            {
                endIsle = world[x, y];
                break;
            }

        }

        return endIsle;
    }

    private void insertAdditionalConnections()
    {
        //AddConnPerIsle = System.Math.Min(AddConnPerIsle, 4);

        IsleAbstract isle;

        int connectionsToAdd = (int)System.Math.Floor(connections.Count * AddConnPerIsle);

        for (int i = 0; i < connectionsToAdd; i++)
        {
            int tmp = rnd.Next(0, isles.Count);

            isle = isles[tmp];

            List<int> directions = new List<int>();

            for (int j = 0; j < 6; j++)
            {
                directions.Add(j);
            }

            for (int j = 0; j < 6; j++)
            {
                tmp = rnd.Next(0, directions.Count);
                int direction = directions[tmp];
                directions.RemoveAt(tmp);

                IsleAbstract islePartner = null;

                switch (direction)
                {
                    case 0:
                        if (isle.PortalUp != null) continue;
                        islePartner = travelUp((int)isle.Index.x, (int)isle.Index.y);
                        break;
                    case 1:
                        if (isle.PortalUpRight != null) continue;
                        islePartner = travelUpRight((int)isle.Index.x, (int)isle.Index.y);
                        break;
                    case 2:
                        if (isle.PortalDownRight != null) continue;
                        islePartner = travelDownRight((int)isle.Index.x, (int)isle.Index.y);
                        break;
                    case 3:
                        if (isle.PortalDown != null) continue;
                        islePartner = travelDown((int)isle.Index.x, (int)isle.Index.y);
                        break;
                    case 4:
                        if (isle.PortalDownLeft != null) continue;
                        islePartner = travelDownLeft((int)isle.Index.x, (int)isle.Index.y);
                        break;
                    case 5:
                        if (isle.PortalUpLeft != null) continue;
                        islePartner = travelUpLeft((int)isle.Index.x, (int)isle.Index.y);
                        break;
                }

                int directionFrom = (direction + 3) % 6;

                if (islePartner != null)
                {
                    connectIsles(isle, islePartner, directionFrom);
                    break;
                }

            }

        }
    }

    public void inserObjectsOnMap()
    {
        List<IsleAbstract> tmpList = new List<IsleAbstract>(this.isles);

        // start Isle
        int tmp = rnd.Next(0, tmpList.Count);
        startIsle = tmpList[tmp];
        startIsle.isleObjectType = IsleAbstract.IsleObjectType.start;
        tmpList.RemoveAt(tmp);

        // keys
        for (int i = 0; i < 3; i++)
        {
            tmp = rnd.Next(0, tmpList.Count);
            tmpList[tmp].isleObjectType = IsleAbstract.IsleObjectType.key;
            tmpList.RemoveAt(tmp);
        }

        // boss
        tmp = rnd.Next(0, tmpList.Count);
        tmpList[tmp].isleObjectType = IsleAbstract.IsleObjectType.boss;
        tmpList.RemoveAt(tmp);

    }

    public List<IsleAbstract> getAllNeighbours()
    {
        List<IsleAbstract> tmpList = new List<IsleAbstract>();

        if (currentIsle.PortalUp != null)
        {
            tmpList.Add(currentIsle.PortalUp.ConnectecPortal.isleAbstract);
        }
        if (currentIsle.PortalUpRight != null)
        {
            tmpList.Add(currentIsle.PortalUpRight.ConnectecPortal.isleAbstract);
        }
        if (currentIsle.PortalDownRight != null)
        {
            tmpList.Add(currentIsle.PortalDownRight.ConnectecPortal.isleAbstract);
        }
        if (currentIsle.PortalDown != null)
        {
            tmpList.Add(currentIsle.PortalDown.ConnectecPortal.isleAbstract);
        }
        if (currentIsle.PortalDownLeft != null)
        {
            tmpList.Add(currentIsle.PortalDownLeft.ConnectecPortal.isleAbstract);
        }
        if (currentIsle.PortalUpLeft != null)
        {
            tmpList.Add(currentIsle.PortalUpLeft.ConnectecPortal.isleAbstract);
        }


        return tmpList;
    }

    public IsleAbstract getRandomIsle()
    {
        IsleAbstract isle = null;

        int tmp = rnd.Next(0, isles.Count);

        isle = isles[tmp];

        return isle;
    }

    public void setCurrentIsle(IsleAbstract isle)
    {
        currentIsle = isle;
    }

    public IsleAbstract getCurrentIsle()
    {
        return currentIsle;
    }

    public void setStartIsle(IsleAbstract isle)
    {
        startIsle = isle;
    }

    public IsleAbstract getStartIsle()
    {
        return startIsle;
    }

    public IsleAbstract[,] getWorld()
    {
        return world;
    }

    public List<ConnectionAbstract> getConnections()
    {
        return connections;
    }
}
