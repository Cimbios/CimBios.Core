using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CimBios.CimModel.CimDatatypeLib
{
    [CimClass("http://iec.ch/TC57/CIM100#IdentifiedObject")]
    public partial class IdentifiedObject : ModelObject
    {
        public IdentifiedObject(DataFacade objectData) : base(objectData)
        {
        }

        public string name
        {
            get => ObjectData.GetAttribute<string>("IdentifiedObject.name");
            set => ObjectData.SetAttribute("IdentifiedObject.name", value);
        }
    }

    [CimClass("http://iec.ch/TC57/CIM100#PowerSystemResource")]
    public partial class PowerSystemResource : IdentifiedObject
    {
        public PowerSystemResource(DataFacade objectData) : base(objectData)
        {
        }


    }

    [CimClass("http://iec.ch/TC57/CIM100#ConnectivityNodeContainer")]
    public partial class ConnectivityNodeContainer : PowerSystemResource
    {
        public ConnectivityNodeContainer(DataFacade objectData) : base(objectData)
        {
        }


    }

    [CimClass("http://iec.ch/TC57/CIM100#EquipmentContainer")]
    public partial class EquipmentContainer : ConnectivityNodeContainer
    {
        public EquipmentContainer(DataFacade objectData) : base(objectData)
        {
        }


    }

    [CimClass("http://iec.ch/TC57/CIM100#Line")]
    public partial class Line : EquipmentContainer
    {
        public Line(DataFacade objectData) : base(objectData)
        {
        }


    }

    [CimClass("http://iec.ch/TC57/CIM100#Substation")]
    public partial class Substation : EquipmentContainer
    {
        public Substation(DataFacade objectData) : base(objectData)
        {
        }


    }
}
