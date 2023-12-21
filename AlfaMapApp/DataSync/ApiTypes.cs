using System;
using System.Collections.Generic;

namespace AlfaMap.DataSync {
    public class Place {
        public int id { get; set; }
        public int cityId { get; set; }
        public string address { get; set; }
        public string name { get; set; }
        public Office[] offices { get; set; }
    }

    public class Office {
        public int id { get; set; }
        public int placeId { get; set; }
        public int typeId { get; set; }
        public string name { get; set; }
        public Node node { get; set; }
    }

    public class Building {
        public int id { get; set; }
        public int placeId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int? currentModelId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public Place place { get; set; }
    }

    public class BuildingCreate {
        public int placeId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class BuildingUpdate {
        public string name { get; set; }
        public string description { get; set; }
    }

    public class BuildingModel {
        public int id { get; set; }
        public int buildingId { get; set; }
        public string description { get; set; }
        public List<ModelElement> elements { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class BuildingModelCreate {
        public int buildingId { get; set; }
        public string description { get; set; }
        public List<ModelElement> elements { get; set; }
        public bool asCurrent { get; set; }
        public BuildingModelGeometry geometry { get; set; }
    }

    public class BuildingModelUpdate {
        public string description { get; set; }
        public List<ModelElement> elements { get; set; }
        public bool asCurrent { get; set; }
        public BuildingModelGeometry geometry { get; set; }
    }

    public class BuildingModelGeometry {
        public string d2 { get; set; }
        public string d3 { get; set; }
    }

    public class NodeType {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class NodeTotals {
        public double totalArea { get; set; }
        public int workplaceCount { get; set; }
        public int freeWorkplaceCount { get; set; }
        public int employeeCount { get; set; }
    }

    public class EmployeeType {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Employee {
        public string id { get; set; }
        public int workplaceId { get; set; }
        public string username { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public bool isStaff { get; set; }
        public EmployeeType position { get; set; }
        public EmployeeType fLeader { get; set; }
        public EmployeeType aLeader { get; set; }
        public EmployeeType[] departments { get; set; }
    }

    public class Node {
        public int id { get; set; }
        public int officeId { get; set; }
        public int parentId { get; set; }
        public NodeType type { get; set; }
        public NodeType roomType { get; set; }
        public NodeType workplaceType { get; set; }
        public string name { get; set; }
        public string childrenPrefix { get; set; }
        public DateTime updatedAt { get; set; }
        public NodeTotals totals { get; set; }
        public Employee[] employees { get; set; }
    }

    public class ModelElement {
        public string id { get; set; }
        public int? nodeId { get; set; }
        public string type { get; set; } = "Entity";
        public string name { get; set; } = null;
        public string parentId { get; set; } = null;
        public string levelId { get; set; } = null;
        public List<string> path { get; set; } = new List<string>();
        public List<string> childrenIds { get; set; } = null;
        public ElementData data { get; set; }
        public Node node { get; set; }
    }

    public class ElementData { }
}
