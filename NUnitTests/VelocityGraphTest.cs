﻿using Frontenac.Blueprints;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityGraph;
using ElementId = System.UInt32;
using VertexId = System.UInt32;
using EdgeId = System.UInt32;
using PropertyTypeId = System.Int32;
using PropertyId = System.Int32;
using VertexTypeId = System.Int32;
using EdgeTypeId = System.Int32;
using EdgeIdVertexId = System.UInt64;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace NUnitTests
{
  [TestFixture]
  public class VelocityGraphTest
  {
    static readonly string drive = "D:\\";
    static readonly string systemDir = Path.Combine(drive, "VelocityGraphNunit");
    static readonly string inputData = Path.Combine(drive, "bfs-1-socialgraph-release");
    static readonly string licenseDbFile = Path.Combine(drive, "4.odb");
    const int numberOfUserVertices = 4194304;
    const int numberOfLocationVertices = 10000;

    [TestCase(false)]
    public void Create1Vertices(bool vertexIdSetPerVertexType)
    {
      DataCache.MaximumMemoryUse = 10000000000; // 10 GB
      bool dirExist = Directory.Exists(systemDir);
      try
      {
        if (Directory.Exists(systemDir))
          Directory.Delete(systemDir, true); // remove systemDir from prior runs and all its databases.
        Directory.CreateDirectory(systemDir);
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"));
      }
      catch
      {
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"));
      }

      using (SessionNoServer session = new SessionNoServer(systemDir, 5000, false, true))
      {
        session.BeginUpdate();
        Graph g = new Graph(session, vertexIdSetPerVertexType);
        session.Persist(g);
        VertexType userType = g.NewVertexType("User");
        VertexType otherType = g.NewVertexType("Other");
        PropertyType userNamePropertyType = g.NewVertexProperty(userType, "NAME", DataType.String, PropertyKind.Indexed);
        VertexType powerUserType = g.NewVertexType("PowerUser", userType);
        EdgeType userFriendEdgeType = g.NewEdgeType("Friend", true, userType, userType);
        EdgeType userBestFriendEdgeType = g.NewEdgeType("Best Friend", true, userType, userType, userFriendEdgeType);
        EdgeType otherEdgeType = g.NewEdgeType("Other", true, userType, userType);
        PropertyType bestFriendPropertyType = g.NewEdgeProperty(userFriendEdgeType, "START", DataType.DateTime, PropertyKind.Indexed);
        Vertex kinga = userType.NewVertex();
        Vertex robin = userType.NewVertex();
        Vertex mats = powerUserType.NewVertex();
        Vertex chiran = powerUserType.NewVertex();
        Vertex other = otherType.NewVertex();
        Edge bestFriend = kinga.AddEdge(userBestFriendEdgeType, robin);
        Edge otherEdge = kinga.AddEdge(otherEdgeType, robin);
        DateTime now = DateTime.UtcNow;
        mats.SetProperty("Address", 1);
        bestFriend.SetProperty(bestFriendPropertyType, now);
        kinga.SetProperty(userNamePropertyType, "Kinga");
        if (g.VertexIdSetPerType == false)
          mats.SetProperty(userNamePropertyType, "Mats");
        else
        {
          try
          {
            mats.SetProperty(userNamePropertyType, "Mats");
            Assert.Fail("Invalid property for VertexType not handled");
          }
          catch (Exception)
          {
          }
        }
        try
        {
          other.SetProperty(userNamePropertyType, "Mats");
          Assert.Fail("Invalid property for VertexType not handled");
        }
        catch (Exception)
        {
        }       
        try
        {
          otherEdge.SetProperty(bestFriendPropertyType, now);
          Assert.Fail("Invalid property for VertexType not handled");
        }
        catch (Exception)
        {
        }
        Vertex findMats = userNamePropertyType.GetPropertyVertex("Mats", true);
        var list = userNamePropertyType.GetPropertyVertices("Mats", true).ToList();
        //Edge findWhen = bestFriendPropertyType.GetPropertyEdge(now);
        //var list2 = bestFriendPropertyType.GetPropertyEdges(now);
        Console.WriteLine(findMats);
       // session.Commit();
       // session.BeginRead();
        PropertyType adressProperty = g.FindVertexProperty(powerUserType, "Address");
        Vertex find1 = adressProperty.GetPropertyVertex(1, true);
        session.Abort();
        /*session.BeginUpdate();
        g.Unpersist(session);
        session.Commit();
        dirExist = Directory.Exists(systemDir);
        try
        {
          if (Directory.Exists(systemDir))
            Directory.Delete(systemDir, true); // remove systemDir from prior runs and all its databases.
          Directory.CreateDirectory(systemDir);
          File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"));
        }
        catch
        {
          File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"));
        }*/

      }

      using (SessionNoServer session = new SessionNoServer(systemDir, 5000, false, true))
      {
        session.BeginUpdate();
        session.DefaultDatabaseLocation().CompressPages = PageInfo.compressionKind.None;
        Graph g = new Graph(session, vertexIdSetPerVertexType);
        session.Persist(g);
        Graph g2 = new Graph(session);
        session.Persist(g2);
        Graph g3 = new Graph(session);
        session.Persist(g3);
        UInt32 dbNum = session.DatabaseNumberOf(typeof(Graph));
        Graph g4 = (Graph)session.Open(dbNum, 2, 1, true); // g4 == g
        Graph g5 = (Graph)session.Open(dbNum, 2, 2, true); // g5 == g2
        Graph g6 = (Graph)session.Open(dbNum, 2, 3, true); // g6 == g3
        for (int i = 4; i < 8; i++)
        {
          Graph gt = new Graph(session);
          session.Persist(gt);
        }
        Graph g7 = new Graph(session);
        Placement place = new Placement(dbNum, 15);
        session.Persist(place, g7);
        // SCHEMA
        VertexType userType = g.NewVertexType("User");
        VertexType locationType = g.NewVertexType("Location");
        VertexType aVertexType = g.NewVertexType("A");
        VertexType bVertexType = g.NewVertexType("B");
        VertexType cVertexType = g.NewVertexType("C");
        EdgeType uEdge = g.NewEdgeType("unrestricted", true);
        Vertex aVertex = g.NewVertex(aVertexType);
        Vertex bVertex = g.NewVertex(bVertexType);
        Vertex cVertex = g.NewVertex(cVertexType);
        Edge abEdge = (Edge)aVertex.AddEdge("unrestricted", bVertex);
        Edge bcEdge = (Edge)aVertex.AddEdge("unrestricted", cVertex);
        Dictionary<Vertex, HashSet<Edge>> traverse = aVertex.Traverse(uEdge, Direction.Out);
        abEdge.Remove();
        Dictionary<Vertex, HashSet<Edge>> traverse2 = aVertex.Traverse(uEdge, Direction.Out);

        EdgeType friendEdgeType = g.NewEdgeType("Friend", true, userType, userType);
        EdgeType userLocationEdgeType = g.NewEdgeType("UserLocation", true, userType, locationType);

        // DATA
        Random rand = new Random(5);
        for (int i = 0; i < numberOfUserVertices / 100; i++)
        {
          int vId = rand.Next(numberOfUserVertices);
          try
          {
            if (g.VertexIdSetPerType)
              userType.GetVertex(vId);
            else
              g.GetVertex(vId);
            try
            {
              userType.NewVertex(vId);
              Assert.Fail();
            }
            catch (VertexAllreadyExistException)
            {

            }
          }
          catch (VertexDoesNotExistException)
          {
            userType.NewVertex(vId);
            userType.GetVertex(vId);
          }
        }
        for (int i = 0; i < numberOfUserVertices / 10000; i++)
        {
          int vId = rand.Next(numberOfUserVertices);
          try
          {
            Vertex v = userType.GetVertex(vId);
            v.SetProperty("test", 1);
          }
          catch (VertexDoesNotExistException)
          {
          }
        }
        for (int i = 0; i < numberOfUserVertices / 10000; i++)
        {
          int vId = rand.Next(numberOfUserVertices);
          try
          {
            Vertex v = userType.GetVertex(vId);
            userType.RemoveVertex(v);
          }
          catch (VertexDoesNotExistException)
          {
          }
        }
        foreach (Vertex v in userType.GetVertices().ToArray())
          userType.RemoveVertex(v);
        Assert.AreEqual(0, userType.GetVertices().Count());
        for (int i = 100000; i < numberOfUserVertices; i++)
          userType.NewVertex();
        for (int i = 1; i < 100000; i++)
          userType.NewVertex();
        for (int i = 1; i < numberOfLocationVertices; i++)
          locationType.NewVertex();
        session.Commit();
        session.BeginRead();
        foreach (var x in session.AllObjects<BTreeSet<Range<VertexId>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeSet<EdgeType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeSet<EdgeIdVertexId>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<EdgeId, VelocityDbList<ElementId>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<string, PropertyType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<string, EdgeType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<string, VertexType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<EdgeType, BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        session.Commit();
        Validate();
      }
    }

    void Validate()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir, 5000, false, true))
      {
        session.BeginRead();
        Graph g = Graph.Open(session); // it takes a while to open graph fresh from databases
        VertexType userType = g.FindVertexType("User");
        VertexType locationType = g.FindVertexType("Location");
        EdgeType friendEdgeType = g.FindEdgeType("Friend");
        EdgeType userLocationEdgeType = g.FindEdgeType("UserLocation");
        foreach (Edge e in friendEdgeType.GetEdges())
        {
          if (e.Head == e.Tail)
            Console.WriteLine(e.Head + " is friend of itself");
        }
        session.Commit();
      }
    }

    [Test]
    public void CreateEdges()
    {
      Create1Vertices(true);
      using (SessionNoServer session = new SessionNoServer(systemDir, 5000, false, true))
      {
        session.BeginUpdate();
        Graph g = Graph.Open(session); // it takes a while to open graph fresh from databases
        VertexType userType = g.FindVertexType("User");
        VertexType locationType = g.FindVertexType("Location");
        EdgeType friendEdgeType = g.FindEdgeType("Friend");
        EdgeType userLocationEdgeType = g.FindEdgeType("UserLocation");
        int lineNumber = 0;
        long fiendsCt = 0;
        foreach (string line in File.ReadLines(inputData))
        {
          string[] fields = line.Split(' ');
          Vertex aUser = new Vertex(g, userType, ++lineNumber);
          int locationCt = 1;
          foreach (string s in fields)
          {
            if (s.Length > 0)
            {
              ++fiendsCt;
              Vertex aFriend = new Vertex(g, userType, int.Parse(s));
              Vertex aLocation = new Vertex(g, locationType, locationCt++);
              friendEdgeType.NewEdge(aUser, aFriend);
              userLocationEdgeType.NewEdge(aUser, aLocation);
            }
          }
          if (lineNumber >= 5000)
            break;
        }
        Console.WriteLine("Done importing " + lineNumber + " users with " + fiendsCt + " friends");
        session.Commit();
        session.BeginRead();
        foreach (var x in session.AllObjects<BTreeSet<Range<VertexId>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeSet<EdgeType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeSet<EdgeIdVertexId>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<EdgeId, VelocityDbList<ElementId>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<string, PropertyType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<string, EdgeType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<string, VertexType>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        foreach (var x in session.AllObjects<BTreeMap<EdgeType, BTreeMap<VertexType, BTreeMap<VertexId, BTreeSet<EdgeIdVertexId>>>>>(false, true))
          Assert.True(x.ToDoBatchAddCount == 0);
        session.Commit();
        Validate();
      }
    }

    [Test]
    public void DeleteLocationVertices()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir, 5000, false, true))
      {
        session.BeginUpdate();
        Graph g = Graph.Open(session); // it takes a while to open graph fresh from databases
        if (g != null)
        {
          VertexType userType = g.FindVertexType("User");
          VertexType locationType = g.FindVertexType("Location");
          EdgeType friendEdgeType = g.FindEdgeType("Friend");
          EdgeType userLocationEdgeType = g.FindEdgeType("UserLocation");
          for (int i = 1; i < numberOfLocationVertices; i++)
            locationType.RemoveVertex(new Vertex(g, locationType, i));
          Assert.IsTrue(userLocationEdgeType.CountEdges() == 0);
          Assert.IsTrue(userType.GetEdges(userLocationEdgeType, Direction.Out).Count() == 0);
          session.Commit();
          Validate();
        }
      }
    }

    [Test]
    public void DeleteUserVertices()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir, 5000, false, true))
      {
        session.BeginUpdate();
        Graph g = Graph.Open(session); // it takes a while to open graph fresh from databases
        VertexType userType = g.FindVertexType("User");
        VertexType locationType = g.FindVertexType("Location");
        EdgeType friendEdgeType = g.FindEdgeType("Friend");
        EdgeType userLocationEdgeType = g.FindEdgeType("UserLocation");
        for (int i = 1; i < numberOfUserVertices; i++)
          userType.RemoveVertex(new Vertex(g, userType, i));
        Assert.IsTrue(friendEdgeType.GetEdges().Count() == 0);
        session.Commit();
        Validate();
      }
    }

    static string systemHost = Dns.GetHostName();

    [TestCase(false)]
    public void SandeepGraph(bool useServerSession)
    {
      bool dirExist = Directory.Exists(systemDir);
      try
      {
        if (Directory.Exists(systemDir))
          Directory.Delete(systemDir, true); // remove systemDir from prior runs and all its databases.
        Directory.CreateDirectory(systemDir);
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"));
      }
      catch
      {
        File.Copy(licenseDbFile, Path.Combine(systemDir, "4.odb"));
      }
      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        session.DefaultDatabaseLocation().CompressPages = PageInfo.compressionKind.None;
        Graph g = new Graph(session);
        session.Persist(g);

        // SCHEMA
        VertexType userType = g.NewVertexType("User");
        // Add a node type for the movies, with a unique identifier and two indexed Propertys
        VertexType movieType = g.NewVertexType("MOVIE");
        PropertyType movieTitleType = g.NewVertexProperty(movieType, "TITLE", DataType.String, PropertyKind.Indexed);
        PropertyType movieYearType = g.NewVertexProperty(movieType, "YEAR", DataType.Integer, PropertyKind.Indexed);
        PropertyType objectPropertyType = g.NewVertexProperty(movieType, "object", DataType.Object, PropertyKind.NotIndexed);
        PropertyType objectPropertyTypeIndexed = g.NewVertexProperty(movieType, "object2", DataType.IOptimizedPersistable, PropertyKind.Indexed);

        Vertex mVickyCB = movieType.NewVertex();
        mVickyCB.SetProperty(movieTitleType, "Vicky Cristina Barcelona");
        mVickyCB.SetProperty(movieYearType, (int)(2008));
        OptimizedPersistable pObj = new OptimizedPersistable();
        session.Persist(pObj);
        mVickyCB.SetProperty(objectPropertyType, pObj);
        pObj = new OptimizedPersistable();
        session.Persist(pObj);
        mVickyCB.SetProperty(objectPropertyTypeIndexed, pObj);
        Vertex mMatsCB = movieType.NewVertex();
        mMatsCB.SetProperty(movieTitleType, "Mats Cristina Barcelona");
        mMatsCB.SetProperty(movieYearType, (int)(2008));
        pObj = new OptimizedPersistable();
        session.Persist(pObj);
        mMatsCB.SetProperty(objectPropertyType, pObj);
        session.Commit();
        session.BeginUpdate();
        try
        {
          mMatsCB.SetProperty(objectPropertyTypeIndexed, null);
          throw new UnexpectedException();
        }
        catch (NullObjectException)
        {

        }
        mMatsCB.Remove();
        session.Commit();
        //session.Persist(g);
        //session.Commit();
      }

      using (SessionBase session = useServerSession ? (SessionBase)new ServerClientSession(systemDir) : (SessionBase)new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Graph g = Graph.Open(session);
        VertexType movieType = g.FindVertexType("MOVIE");
        Assert.NotNull(movieType);
      }
      Task taskB = new Task(() => WatchUser());
      taskB.Start();
      Task taskA = new Task(() => CreateUser());
      taskA.Start();
      taskB.Wait();
      taskA.Wait();
    }

    public static void CreateUser()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost))
      {
        session.BeginUpdate();
        Graph g = Graph.Open(session);
        VertexType movieType = g.FindVertexType("MOVIE");
        PropertyType movieTitleType = movieType.FindProperty("TITLE");
        PropertyType movieYearType = movieType.FindProperty("YEAR");

        for (int i = 1; i < 50; i++)
        {
          Vertex mVickyCB = movieType.NewVertex();
          mVickyCB.SetProperty(movieTitleType, "Vicky Cristina Barcelona" + i);
          mVickyCB.SetProperty(movieYearType, (int)(2008 + i));
          session.Commit();
          session.BeginUpdate();
          Thread.Sleep(1000);
        }
      }
    }

    public static void WatchUser()
    {
      using (ServerClientSession session = new ServerClientSession(systemDir, systemHost, 1000, true, false))
      {
        session.BeginRead();
        session.SubscribeToChanges(typeof(Vertex));
        Graph g = Graph.Open(session);
        session.Commit();
        Thread.Sleep(5000);

        for (int i = 0; i < 50; i++)
        {
          List<Oid> changes = session.BeginReadWithEvents();
          if (changes.Count == 0)
          {
            Console.WriteLine("No changes events at: " + DateTime.Now.ToString("HH:mm:ss:fff"));
            Thread.Sleep(1000);
          }
          foreach (Oid id in changes)
          {
            object obj = session.Open(id);
            Console.WriteLine("Received change event for: " + obj + " at: " + DateTime.Now.ToString("HH:mm:ss:fff")); ;
            //session.UnsubscribeToChanges(typeof(Person));
          }
          session.Commit();
          Thread.Sleep(2000);
        }
      }
    }

    // [Test]
    public void TestVelecoityBuildLocal()
    {
      using (var session = new SessionNoServer(@"d:\graphtest"))
      {
        session.BeginUpdate();
        session.DefaultDatabaseLocation().CompressPages = PageInfo.compressionKind.LZ4;
        DataCache.MaximumMemoryUse = 2000000000;
        //var dbl = new DatabaseLocation(Dns.GetHostEntry("wordanalysis.cloudapp.net").HostName, @"Z:\DBStore\", 0,
        //    3, session);
        //DatabaseLocation bl = session.NewLocation(dbl);
        //session.Commit(false);   
        //        Assert.That(bl!=null);

        var graph = new Graph(session);
        session.Persist(graph);
        //define schema
        // session.BeginUpdate();
        VertexType namedScore = graph.NewVertexType("NamedScore");
        PropertyType name = namedScore.NewProperty("Name", DataType.String, PropertyKind.Indexed);
        PropertyType int_score = namedScore.NewProperty("IntScore", DataType.Integer,
            PropertyKind.NotIndexed);
        PropertyType double_score = namedScore.NewProperty("DoubleScore", DataType.Double,
            PropertyKind.NotIndexed);

        VertexType concept = graph.NewVertexType("Concept");
        PropertyType conceptName = concept.NewProperty("Name", DataType.String, PropertyKind.Unique);
        PropertyType conceptSize = concept.NewProperty("ConceptSize", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType conceptFrequency = concept.NewProperty("ConceptFrequency", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType similarity = concept.NewProperty("Similarity", DataType.Double, PropertyKind.NotIndexed);
        PropertyType vagueness = concept.NewProperty("Vagueness", DataType.Double, PropertyKind.NotIndexed);

        VertexType instance = graph.NewVertexType("Instance", concept);
        PropertyType instanceSize = instance.NewProperty("InstanceSize", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType instanceFrequency = instance.NewProperty("InstanceFrequency", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType instanceName = instance.NewProperty("Name", DataType.String, PropertyKind.Unique);

        VertexType attributes = graph.NewVertexType("Attribute", namedScore);

        //EdgeType hasAttirbute = attributes.edgattributes.NewHeadToTailEdge(nameScorePair,);
        EdgeType cooccursWith = graph.NewEdgeType("CooccursWith", true, instance, instance);
        PropertyType coocurrenceFrequency = namedScore.NewProperty("IntScore", DataType.Integer, PropertyKind.NotIndexed);

        VertexType synonym = graph.NewVertexType("Synonym", namedScore);

        PropertyType synonymScore = synonym.NewProperty("Score", DataType.Integer, PropertyKind.NotIndexed);
        EdgeType hasSynonym = graph.NewEdgeType("HasSynonym", true, synonym, instance);


        EdgeType isA = graph.NewEdgeType("IsA", true, concept, instance);
        PropertyType frequency = isA.NewProperty("frequency", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType popularity = isA.NewProperty("popularity", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType ZipfSlope = isA.NewProperty("zipf_slope", DataType.Double, PropertyKind.NotIndexed);
        PropertyType ZipfPearson = isA.NewProperty("zipf_pearson", DataType.Double, PropertyKind.NotIndexed);
        EdgeType cooccurence = graph.NewEdgeType("Coocurrence", true, concept, concept);
        //Vertex vx1 = graph.NewVertex(instance);
        //vx1.SetProperty("Name", "bla");

        //Vertex vx2 = graph.NewVertex(instance);
        //vx2.SetProperty("bla", "name");
        //Edge edge = graph.NewEdge(cooccurence, vx1, vx2);

        using (var llz = new StreamReader(@"d:\isa_core.txt"))
        {
          string lastConcept = string.Empty;
          while (!llz.EndOfStream)
          {
            string ln = llz.ReadLine();
            if (string.IsNullOrEmpty(ln)) continue;
            string[] items = ln.Split(new[] { '\t' });

            if (items.Length < 4) continue;

            int id = -1;
            if (!int.TryParse(items[2], out id)) continue;
            var conceptVertex = graph.FindVertex(conceptName, items[0], false);
            if (conceptVertex == null)
            {
              conceptVertex = graph.NewVertex(concept);
              conceptVertex.SetProperty(conceptName, items[0]);
              conceptVertex.SetProperty(conceptFrequency, int.Parse(items[4]));
              conceptVertex.SetProperty(conceptSize, int.Parse(items[5]));
              double d = double.NaN;
              double.TryParse(items[6], out d);
              conceptVertex.SetProperty(vagueness, d);
              d = double.NaN;
              double.TryParse(items[7], out d);
              conceptVertex.SetProperty(ZipfSlope, d);
              d = double.NaN;
              double.TryParse(items[8], out d);
              conceptVertex.SetProperty(ZipfPearson, d);
            }
            var instanceVertex = graph.FindVertex(instanceName, items[1], false);

            if (instanceVertex == null)
            {
              instanceVertex = graph.NewVertex(instance);
              instanceVertex.SetProperty(instanceFrequency, int.Parse(items[9]));
              instanceVertex.SetProperty(instanceSize, int.Parse(items[10]));
            }
            var isaedge = graph.NewEdge(isA, conceptVertex, instanceVertex);
            isaedge.SetProperty(frequency, int.Parse(items[2]));
            isaedge.SetProperty(popularity, int.Parse(items[3]));
          }
          session.Commit();
        }
      }
    }

    [Test]
    public void AddVertices()
    {
      using (var session = new SessionNoServer(@"d:\graphtest2"))
      {
        session.DefaultDatabaseLocation().CompressPages = PageInfo.compressionKind.LZ4;
        DataCache.MaximumMemoryUse = 4000000000;
        var sw = new Stopwatch();
        sw.Start();
        //int i = 0;
        //var dbl = new DatabaseLocation(Dns.GetHostEntry("wordanalysis.cloudapp.net").HostName, @"Z:\DBStore\", 0,
        //    3, session);
        //DatabaseLocation bl = session.NewLocation(dbl);
        //session.Commit(false);   
        //        Assert.That(bl!=null);

        // var db = session.OpenDatabase(15, true);
        var graph = new Graph(session);
        session.BeginUpdate();
        session.Persist(graph);

        //define schema                       Trace.Wri


        VertexType concept = graph.NewVertexType("Concept");
        PropertyType conceptName = concept.NewProperty("ConceptName", DataType.String, PropertyKind.Unique);
        PropertyType conceptSize = concept.NewProperty("ConceptSize", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType conceptFrequency = concept.NewProperty("ConceptFrequency", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType similarity = concept.NewProperty("Similarity", DataType.Double, PropertyKind.NotIndexed);
        PropertyType vagueness = concept.NewProperty("Vagueness", DataType.Double, PropertyKind.NotIndexed);

        VertexType instance = graph.NewVertexType("Instance", concept);
        PropertyType instanceSize = instance.NewProperty("InstanceSize", DataType.Integer,
            PropertyKind.NotIndexed);
        PropertyType instanceName = instance.NewProperty("InstanceName", DataType.String,
            PropertyKind.Unique);
        PropertyType instanceFrequency = instance.NewProperty("InstanceFrequency",
            DataType.Integer,
            PropertyKind.NotIndexed);

        //VertexType attributes = graph.NewVertexType("Attribute");

        ////EdgeType hasAttirbute = attributes.edgattributes.NewHeadToTailEdge(nameScorePair,);
        //EdgeType cooccursWith = graph.NewEdgeType("CooccursWith", true, instance, instance);
        //PropertyType coocurrenceFrequency = namedScore.NewProperty("IntScore", DataType.Integer,
        //    PropertyKind.NotIndexed);

        //VertexType synonym = graph.NewVertexType("Synonym", namedScore);

        //PropertyType synonymScore = synonym.NewProperty("Score", DataType.Integer, PropertyKind.NotIndexed);
        //EdgeType hasSynonym = graph.NewEdgeType("HasSynonym", true, synonym, instance);


        EdgeType isA = graph.NewEdgeType("IsA", true, concept, instance);
        PropertyType frequency = isA.NewProperty("frequency", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType popularity = isA.NewProperty("popularity", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType ZipfSlope = isA.NewProperty("zipf_slope", DataType.Double, PropertyKind.NotIndexed);
        PropertyType ZipfPearson = isA.NewProperty("zipf_pearson", DataType.Double, PropertyKind.NotIndexed);
        EdgeType cooccurence = graph.NewEdgeType("Coocurrence", true, concept, concept);
        //LRVertex vx1 = graph.NewVertex(instance);
        //vx1.SetProperty("Name", "bla");

        //LRVertex vx2 = graph.NewVertex(instance);
        //vx2.SetProperty("bla", "name");
        //LREdge edge = graph.NewEdge(cooccurence, vx1, vx2);

        Vertex v2 = graph.NewVertex(concept);


        v2.SetProperty(conceptName, "factor");
        Vertex v3 = graph.NewVertex(instance);

        v3.SetProperty(instanceName, "age");


        session.Commit();
      }
      using (var session = new SessionNoServer(@"d:\graphtest2"))
      {
        //session.DefaultDatabaseLocation().CompressPages = true;
        DataCache.MaximumMemoryUse = 4000000000;
        var sw = new Stopwatch();
        sw.Start();
        //int i = 0;

        session.BeginRead();
        var graph = Graph.Open(session);

        //define schema                       Trace.Wri


        VertexType[] vertexTypes = graph.FindVertexTypes();
        //vertexTypes.Select(x => x.TypeName).PrintDump();
        EdgeType[] edgeTypes = graph.FindEdgeTypes();

        VertexType concept = vertexTypes.FirstOrDefault(x => x.TypeName == "Concept") ?? graph.NewVertexType("Concept");

        PropertyType conceptName = concept.FindProperty("ConceptName");
        Assert.IsNotNull(conceptName, "ConceptName");
        PropertyType conceptSize = concept.FindProperty("ConceptSize");
        Assert.IsNotNull(conceptSize, "ConceptSize");
        PropertyType conceptFrequency = concept.FindProperty("ConceptFrequency");
        //PropertyType similarity = concept.NewProperty("Similarity", VelocityGraph.DataType.Double,
        //    VelocityGraph.PropertyKind.NotIndexed);
        PropertyType vagueness = concept.FindProperty("Vagueness");//, DataType.Double,PropertyKind.NotIndexed);

        VertexType instance = vertexTypes.FirstOrDefault(x => x.TypeName == "Instance");
        PropertyType instanceSize = instance.FindProperty("InstanceSize");
        PropertyType instanceName = instance.FindProperty("InstanceName");
        PropertyType instanceFrequency = instance.FindProperty("InstanceFrequency");

        //VertexType attributes = graph.NewVertexType("Attribute");

        ////EdgeType hasAttirbute = attributes.edgattributes.NewHeadToTailEdge(nameScorePair,);
        //EdgeType cooccursWith = graph.NewEdgeType("CooccursWith", true, instance, instance);
        //PropertyType coocurrenceFrequency = namedScore.NewProperty("IntScore", DataType.Integer,
        //    PropertyKind.NotIndexed);

        //VertexType synonym = graph.NewVertexType("Synonym", namedScore);

        //PropertyType synonymScore = synonym.NewProperty("Score", DataType.Integer, PropertyKind.NotIndexed);
        //EdgeType hasSynonym = graph.NewEdgeType("HasSynonym", true, synonym, instance);


        EdgeType isA = edgeTypes.FirstOrDefault(x => x.TypeName == "IsA");
        PropertyType frequency = isA.FindProperty("frequency");
        PropertyType popularity = isA.FindProperty("popularity");
        PropertyType ZipfSlope = isA.FindProperty("zipf_slope");
        PropertyType ZipfPearson = isA.FindProperty("zipf_pearson");
        EdgeType cooccurence = graph.FindEdgeType("Coocurrence");
        //LRVertex vx1 = graph.NewVertex(instance);
        //vx1.SetProperty("Name", "bla");

        //LRVertex vx2 = graph.NewVertex(instance);
        //vx2.SetProperty("bla", "name");
        //LREdge edge = graph.NewEdge(cooccurence, vx1, vx2);



        Assert.IsNotNull(conceptName);

        Vertex f = graph.FindVertex(conceptName, "factor");
        var inst = graph.FindVertex(instanceName, "age");
        Assert.IsNotNull(f);
        Assert.IsNotNull(inst);
      }
      //if (instanceVertex == null)
    }
  }
}