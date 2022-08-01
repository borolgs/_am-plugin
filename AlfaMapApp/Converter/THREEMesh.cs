using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Common;
using RevitWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Converter
{
    public class THREEMesh: THREEObject
    {
        public override string type => "Mesh";
        public override string name { get; set; }
        public override string uuid { get; set; }
        public override double[] matrix { get; set; } = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        public override THREEUserData userData { get; set; }

        public string geometry { get; set; }
        public string material { get; set; }
        public bool castShadow { get; set; } = true;
        public bool receiveShadow { get; set; } = true;

        public static THREEMesh Create(
            string name,
            string uuid,
            string geometryUUID,
            string materialUUID,
            ElementType type)
        {
            var mesh = new THREEMesh
            {
                name = name,
                uuid = uuid,
                geometry = geometryUUID,
                material = materialUUID
            };

            mesh.userData = new THREEEntityData
            {
                type = type,
            };

            return mesh;

        }

        public static THREEMesh FromRoom(
            Room element,
            string geometryUUID = null,
            string materialUUID = THREEMaterial.DefaultTransparentMaterialUuid
            )
        {
            var mesh = new THREEMesh
            {
                name = element.UniqueId,
                uuid = element.UniqueId,
                geometry = geometryUUID,
                material = materialUUID
            };

            XYZ pt = (element.Location as LocationPoint).Point;
            var origin = new[] { Utils.FootToMeter(pt.Y), Utils.FootToMeter(pt.Z), Utils.FootToMeter(pt.X) };

            mesh.userData = new THREEEntityData
            {
                type = ElementType.Room,
                name = element.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString(),
                origin = origin
            };

            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox != null)
            {
                mesh.userData.elevation = Utils.FootToMeter(bbox.Min.Z);
            }

            return mesh;
        }

        public static THREEMesh FromLevel(
            Level element,
            string geometryUUID = null,
            string materialUUID = THREEMaterial.DefaultMaterialUuid,
            string suffix = "",
            ElementType type = ElementType.Element
            )
        {
            var mesh = new THREEMesh
            {
                name = $"{element.UniqueId}__MergedElements__{suffix}",
                geometry = geometryUUID,
                material = materialUUID
            };

            mesh.userData = new THREEEntityData
            {
                type = type,
                elevation = Utils.FootToMeter(element.Elevation),
                name = "MergedElement"
            };
            
            return mesh;
        }

        public static THREEMesh FromWorkplace(
            FamilyInstance element,
            string geometryUUID = null,
            string materialUUID = THREEMaterial.DefaultMaterialUuid
        )
        {
            var mesh = new THREEMesh
            {
                name = element.UniqueId,
                uuid = element.UniqueId,
                geometry = geometryUUID,
                material = materialUUID
            };

            mesh.userData = new THREEEntityData
            {
                type = ElementType.Workplace,
                name = element.get_Parameter(OldParams.WorkplaceNumber.Guid).AsString()
            };

            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox != null)
            {
                mesh.userData.elevation = Utils.FootToMeter(bbox.Min.Z);
            }

            return mesh;
        }

        public static THREEMesh FromFamilyInstance(
            FamilyInstance element,
            string geometryUUID = null,
            string materialUUID = THREEMaterial.DefaultMaterialUuid
        )
        {
            var mesh = new THREEMesh
            {
                name = element.UniqueId,
                uuid = element.UniqueId,
                geometry = geometryUUID,
                material = materialUUID
            };

            mesh.userData = new THREEEntityData
            {
                type = ElementType.Entity
            };

            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox != null)
            {
                mesh.userData.elevation = Utils.FootToMeter(bbox.Min.Z);
            }

            return mesh;
        }

        public void UpdateMatrix(FamilyInstance instance)
        {
            bool mirrored = instance.Mirrored;
            XYZ orient = instance.FacingOrientation;

            Transform transform = instance.GetTotalTransform();

            XYZ origin = transform.Origin;

            Matrix translation = Matrix.Translation(
                Utils.FootToMeter(origin.Y),
                Utils.FootToMeter(origin.Z),
                Utils.FootToMeter(origin.X)
            );

            double yAngle = orient.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ);
            double yAngle_ = orient.AngleTo(XYZ.BasisY);

            Matrix rotation = Matrix.RotationY(yAngle);

            Matrix scaling = Matrix.Scaling(1, 1, mirrored ? -1 : 1);
            Matrix transformation = translation.Multiply(rotation).Multiply(scaling);
            matrix = transformation.ToArray();

            //matrix = Matrix.FromTransform(transform, orient).Multiply(rotation).ToArray();
        }
    }
}
