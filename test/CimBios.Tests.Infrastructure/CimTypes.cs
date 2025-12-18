using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.DatatypeLib.TypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tests.Infrastructure;

/// <summary>
/// This is a root class to provide common identification for all classes
/// needing identification and naming attributes.
/// </summary>
[CimClass (ClassUri)]
public partial class IdentifiedObject
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Resource (oid, metaClass)
{
  public new const string ClassUri
      = "http://iec.ch/TC57/CIM100#IdentifiedObject";

  /// <summary>
  /// The aliasName is free text human readable name of the object alternative
  /// to IdentifiedObject.name. It may be non unique and may not correlate to a
  /// naming hierarchy.The attribute aliasName is retained because of backwards
  /// compatibility between CIM relases. It is however recommended to replace
  /// aliasName with the Name class as aliasName is planned for retirement at a
  /// future time.
  /// </summary>
  public string ? aliasName
  {
    get => GetAttribute < string ? > (nameof (aliasName));
    set => SetAttribute (nameof (aliasName), value);
  }
  /// <summary>
  /// The description is a free human readable text describing or naming the
  /// object. It may be non unique and may not correlate to a naming hierarchy.
  /// </summary>
  public string ? description
  {
    get => GetAttribute < string ? > (nameof (description));
    set => SetAttribute (nameof (description), value);
  }
  /// <summary>
  /// Master resource identifier issued by a model authority. The mRID is unique
  /// within an exchange context. Global uniqueness is easily achieved by using
  /// a UUID, as specified in RFC 4122, for the mRID. The use of UUID is
  /// strongly recommended.For CIMXML data files in RDF syntax conforming to IEC
  /// 61970-552, the mRID is mapped to rdf:ID or rdf:about attributes that
  /// identify CIM object elements.
  /// </summary>
  public string ? mRID
  {
    get => GetAttribute < string ? > (nameof (mRID));
    set => SetAttribute (nameof (mRID), value);
  }
  /// <summary>
  /// The name is any free human readable and possibly non unique text naming
  /// the object.
  /// </summary>
  public string ? name
  {
    get => GetAttribute < string ? > (nameof (name));
    set => SetAttribute (nameof (name), value);
  }
}

/// <summary>
/// Connectivity nodes are points where terminals of AC conducting equipment are
/// connected together with zero impedance.
/// </summary>
[CimClass (ClassUri)]
public partial class ConnectivityNode
    (IOIDDescriptor oid, ICimMetaClass metaClass)
    : IdentifiedObject (oid, metaClass)
{
    public new const string ClassUri
        = "http://iec.ch/TC57/CIM100#ConnectivityNode";

    /// <summary>
    /// Container of this connectivity node.
    /// </summary>
    public ConnectivityNodeContainer ? ConnectivityNodeContainer
    {
        get => GetAssoc1To1<ConnectivityNodeContainer> (
            nameof (ConnectivityNodeContainer));
        set => SetAssoc1To1 (nameof (ConnectivityNodeContainer), value);
    }

    /// <summary>
    /// Terminals interconnected with zero impedance at a this connectivity node.
    /// </summary>
    public Terminal[] Terminals => GetAssoc1ToM<Terminal> (nameof (Terminals));
    public void AddToTerminals (Terminal assocObject) => AddAssoc1ToM (
        nameof (Terminals), assocObject);
    public void RemoveFromTerminals (Terminal assocObject) => RemoveAssoc1ToM (
        nameof (Terminals), assocObject);
    public void RemoveAllFromTerminals () => RemoveAllAssocs1ToM (
        nameof (Terminals));
}

/// <summary>
/// Defines a system base voltage which is referenced.
/// </summary>
[CimClass (ClassUri)]
public partial class BaseVoltage
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : IdentifiedObject (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#BaseVoltage";

  /// <summary>
  /// The power system resource's base voltage.  Shall be a positive value and
  /// not zero.
  /// </summary>
  public float ? nominalVoltage
  {
    get => GetAttribute < float ? > (nameof (nominalVoltage));
    set => SetAttribute (nameof (nominalVoltage), value);
  }
  /// <summary>
  ///
  /// </summary>
  public bool ? isDC
  {
    get => GetAttribute < bool ? > (nameof (isDC));
    set => SetAttribute (nameof (isDC), value);
  }

  /// <summary>
  /// All conducting equipment with this base voltage.  Use only when there is
  /// no voltage level container used and only one base voltage applies.  For
  /// example, not used for transformers.
  /// </summary>
  public ConductingEquipment[] ConductingEquipment => GetAssoc1ToM<
      ConductingEquipment> (nameof (ConductingEquipment));
  public void
  AddToConductingEquipment (ConductingEquipment assocObject) => AddAssoc1ToM (
      nameof (ConductingEquipment), assocObject);
  public void RemoveFromConductingEquipment (
      ConductingEquipment
          assocObject) => RemoveAssoc1ToM (nameof (ConductingEquipment),
                                           assocObject);
  public void RemoveAllFromConductingEquipment () => RemoveAllAssocs1ToM (
      nameof (ConductingEquipment));
  
  /// <summary>
  /// The voltage levels having this base voltage.
  /// </summary>
  public VoltageLevel[] VoltageLevel => GetAssoc1ToM<VoltageLevel> (
      nameof (VoltageLevel));
  public void AddToVoltageLevel (VoltageLevel assocObject) => AddAssoc1ToM (
      nameof (VoltageLevel), assocObject);
  public void RemoveFromVoltageLevel (
      VoltageLevel assocObject) => RemoveAssoc1ToM (nameof (VoltageLevel),
                                                    assocObject);
  public void RemoveAllFromVoltageLevel () => RemoveAllAssocs1ToM (
      nameof (VoltageLevel));
}

/// <summary>
/// A power system resource (PSR) can be an item of equipment such as a switch,
/// an equipment container containing many individual items of equipment such as
/// a substation, or an organisational entity such as sub-control area. Power
/// system resources can have measurements associated.
/// </summary>
[CimClass (ClassUri)]
public partial class PowerSystemResource
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : IdentifiedObject (oid, metaClass)
{
  public new const string ClassUri
      = "http://iec.ch/TC57/CIM100#PowerSystemResource";
  
  /// <summary>
  /// All assets represented by this power system resource. For example,
  /// multiple conductor assets are electrically modelled as a single AC line
  /// segment.
  /// </summary>
  public Asset[] Assets => GetAssoc1ToM<Asset> (nameof (Assets));
  public void AddToAssets (Asset assocObject) => AddAssoc1ToM (nameof (Assets),
                                                               assocObject);
  public void RemoveFromAssets (Asset assocObject) => RemoveAssoc1ToM (
      nameof (Assets), assocObject);
  public void RemoveAllFromAssets () => RemoveAllAssocs1ToM (nameof (Assets));
}

/// <summary>
/// The parts of a power system that are physical devices, electronic or
/// mechanical.
/// </summary>
[CimClass (ClassUri)]
public partial class Equipment
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : PowerSystemResource (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#Equipment";

  /// <summary>
  /// Specifies the availability of the equipment under normal operating
  /// conditions. True means the equipment is available for topology processing,
  /// which determines if the equipment is energized or not. False means that
  /// the equipment is treated by network applications as if it is not in the
  /// model.
  /// </summary>
  public bool ? normallyInService
  {
    get => GetAttribute < bool ? > (nameof (normallyInService));
    set => SetAttribute (nameof (normallyInService), value);
  }

  /// <summary>
  /// Container of this equipment.
  /// </summary>
  public EquipmentContainer ? EquipmentContainer
  {
    get => GetAssoc1To1<EquipmentContainer> (nameof (EquipmentContainer));
    set => SetAssoc1To1 (nameof (EquipmentContainer), value);
  }
}

/// <summary>
/// AuxiliaryEquipment describe equipment that is not performing any primary
/// functions but support for the equipment performing the primary
/// function.AuxiliaryEquipment is attached to primary equipment via an
/// association with Terminal.
/// </summary>
[CimClass (ClassUri)]
public partial class AuxiliaryEquipment
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Equipment (oid, metaClass)
{
  public new const string ClassUri
      = "http://iec.ch/TC57/CIM100#AuxiliaryEquipment";

  /// <summary>
  /// The Terminal at the equipment where the AuxiliaryEquipment is attached.
  /// </summary>
  public Terminal ? Terminal
  {
    get => GetAssoc1To1<Terminal> (nameof (Terminal));
    set => SetAssoc1To1 (nameof (Terminal), value);
  }
}

/// <summary>
/// This class describe devices that transform a measured quantity into signals
/// that can be presented at displays, used in control or be recorded.
/// </summary>
[CimClass (ClassUri)]
public partial class Sensor
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : AuxiliaryEquipment (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#Sensor";
}

/// <summary>
/// Instrument transformer used to measure electrical qualities of the circuit
/// that is being protected and/or monitored. Typically used as current
/// transducer for the purpose of metering or protection. A typical secondary
/// current rating would be 5A.
/// </summary>
[CimClass (ClassUri)]
public partial class CurrentTransformer
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Sensor (oid, metaClass)
{
  public new const string ClassUri
      = "http://iec.ch/TC57/CIM100#CurrentTransformer";

  /// <summary>
  /// CT accuracy classification.
  /// </summary>
  public string ? accuracyClass
  {
    get => GetAttribute < string ? > (nameof (accuracyClass));
    set => SetAttribute (nameof (accuracyClass), value);
  }
  /// <summary>
  ///
  /// </summary>
  public bool ? isEmbeded
  {
    get => GetAttribute < bool ? > (nameof (isEmbeded));
    set => SetAttribute (nameof (isEmbeded), value);
  }
  /// <summary>
  ///
  /// </summary>
  public float ? ratedCurrent
  {
    get => GetAttribute < float ? > (nameof (ratedCurrent));
    set => SetAttribute (nameof (ratedCurrent), value);
  }
  /// <summary>
  ///
  /// </summary>
  public float ? ratedSecondaryCurrent
  {
    get => GetAttribute < float ? > (nameof (ratedSecondaryCurrent));
    set => SetAttribute (nameof (ratedSecondaryCurrent), value);
  }
}

/// <summary>
/// The parts of the AC power system that are designed to carry current or that
/// are conductively connected through terminals.
/// </summary>
[CimClass (ClassUri)]
public partial class ConductingEquipment
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Equipment (oid, metaClass)
{
  public new const string ClassUri
      = "http://iec.ch/TC57/CIM100#ConductingEquipment";

  /// <summary>
  ///
  /// </summary>
  public bool ? isThreePhaseEquipment
  {
    get => GetAttribute < bool ? > (nameof (isThreePhaseEquipment));
    set => SetAttribute (nameof (isThreePhaseEquipment), value);
  }

  /// <summary>
  /// Base voltage of this conducting equipment.  Use only when there is no
  /// voltage level container used and only one base voltage applies.  For
  /// example, not used for transformers.
  /// </summary>
  public BaseVoltage ? BaseVoltage
  {
    get => GetAssoc1To1<BaseVoltage> (nameof (BaseVoltage));
    set => SetAssoc1To1 (nameof (BaseVoltage), value);
  }

  /// <summary>
  /// Conducting equipment have terminals that may be connected to other
  /// conducting equipment terminals via connectivity nodes or topological
  /// nodes.
  /// </summary>
  public Terminal[] Terminals => GetAssoc1ToM<Terminal> (nameof (Terminals));
  public void AddToTerminals (Terminal assocObject) => AddAssoc1ToM (
      nameof (Terminals), assocObject);
  public void RemoveFromTerminals (Terminal assocObject) => RemoveAssoc1ToM (
      nameof (Terminals), assocObject);
  public void RemoveAllFromTerminals () => RemoveAllAssocs1ToM (
      nameof (Terminals));
}

/// <summary>
/// A generic device designed to close, or open, or both, one or more electric
/// circuits.  All switches are two terminal devices including grounding
/// switches. The ACDCTerminal.connected at the two sides of the switch shall
/// not be considered for assessing switch connectivity, i.e. only Switch.open,
/// .normalOpen and .locked are relevant.
/// </summary>
[CimClass (ClassUri)]
public partial class Switch
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : ConductingEquipment (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#Switch";

  /// <summary>
  /// The attribute is used in cases when no Measurement for the status value is
  /// present. If the Switch has a status measurement the Discrete.normalValue
  /// is expected to match with the Switch.normalOpen.
  /// </summary>
  public bool ? normalOpen
  {
    get => GetAttribute < bool ? > (nameof (normalOpen));
    set => SetAttribute (nameof (normalOpen), value);
  }
  /// <summary>
  /// The maximum continuous current carrying capacity in amps governed by the
  /// device material and construction.The attribute shall be a positive value.
  /// </summary>
  public float ? ratedCurrent
  {
    get => GetAttribute < float ? > (nameof (ratedCurrent));
    set => SetAttribute (nameof (ratedCurrent), value);
  }
  /// <summary>
  ///
  /// </summary>
  public float ? differenceInTransitTime
  {
    get => GetAttribute < float ? > (nameof (differenceInTransitTime));
    set => SetAttribute (nameof (differenceInTransitTime), value);
  }
}

/// <summary>
/// A manually operated or motor operated mechanical switching device used for
/// isolating a circuit or equipment from ground.
/// </summary>
[CimClass (ClassUri)]
public partial class GroundDisconnector
    (IOIDDescriptor oid, ICimMetaClass metaClass)
    : Switch (oid, metaClass)
{
    public new const string ClassUri
        = "http://iec.ch/TC57/CIM100#GroundDisconnector";
}

/// <summary>
/// A ProtectedSwitch is a switching device that can be operated by
/// ProtectionEquipment.
/// </summary>
[CimClass (ClassUri)]
public partial class ProtectedSwitch
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Switch (oid, metaClass)
{
  public new const string ClassUri
      = "http://iec.ch/TC57/CIM100#ProtectedSwitch";

  /// <summary>
  /// The maximum fault current a breaking device can break safely under
  /// prescribed conditions of use.
  /// </summary>
  public float ? breakingCapacity
  {
    get => GetAttribute < float ? > (nameof (breakingCapacity));
    set => SetAttribute (nameof (breakingCapacity), value);
  }
}

/// <summary>
/// A mechanical switching device capable of making, carrying, and breaking
/// currents under normal circuit conditions and also making, carrying for a
/// specified time, and breaking currents under specified abnormal circuit
/// conditions e.g.  those of short circuit.
/// </summary>
[CimClass (ClassUri)]
public partial class Breaker
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : ProtectedSwitch (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#Breaker";

  /// <summary>
  /// The transition time from open to close.
  /// </summary>
  public float ? inTransitTime
  {
    get => GetAttribute < float ? > (nameof (inTransitTime));
    set => SetAttribute (nameof (inTransitTime), value);
  }
}

/// <summary>
/// A collection of power system resources (within a given substation) including
/// conducting equipment, protection relays, measurements, and telemetry.  A bay
/// typically represents a physical grouping related to modularization of
/// equipment.
/// </summary>
[CimClass (ClassUri)]
public partial class Bay
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : EquipmentContainer (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#Bay";

  /// <summary>
  /// The voltage level containing this bay.
  /// </summary>
  public VoltageLevel ? VoltageLevel
  {
    get => GetAssoc1To1<VoltageLevel> (nameof (VoltageLevel));
    set => SetAssoc1To1 (nameof (VoltageLevel), value);
  }
}

/// <summary>
/// An electrical connection point (AC or DC) to a piece of conducting
/// equipment. Terminals are connected at physical connection points called
/// connectivity nodes.
/// </summary>
[CimClass (ClassUri)]
public partial class ACDCTerminal
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : IdentifiedObject (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#ACDCTerminal";

  /// <summary>
  /// The orientation of the terminal connections for a multiple terminal
  /// conducting equipment.  The sequence numbering starts with 1 and additional
  /// terminals should follow in increasing order.   The first terminal is the
  /// "starting point" for a two terminal branch.
  /// </summary>
  public int ? sequenceNumber
  {
    get => GetAttribute < int ? > (nameof (sequenceNumber));
    set => SetAttribute (nameof (sequenceNumber), value);
  }
}

/// <summary>
/// An AC electrical connection point to a piece of conducting equipment.
/// Terminals are connected at physical connection points called connectivity
/// nodes.
/// </summary>
[CimClass (ClassUri)]
public partial class Terminal
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : ACDCTerminal (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#Terminal";

  /// <summary>
  /// Represents the normal network phasing condition. If the attribute is
  /// missing, three phases (ABC) shall be assumed, except for terminals of
  /// grounding classes (specializations of EarthFaultCompensator,
  /// GroundDisconnector, and Ground) which will be assumed to be N. Therefore,
  /// phase code ABCN is explicitly declared when needed, e.g. for star point
  /// grounding equipment.The phase code on terminals connecting same
  /// ConnectivityNode or same TopologicalNode as well as for equipment between
  /// two terminals shall be consistent.
  /// </summary>
  public PhaseCode ? phases
  {
    get => GetAttribute < PhaseCode ? > (nameof (phases));
    set => SetAttribute (nameof (phases), value);
  }

  /// <summary>
  /// The conducting equipment of the terminal.  Conducting equipment have
  /// terminals that may be connected to other conducting equipment terminals
  /// via connectivity nodes or topological nodes.
  /// </summary>
  public ConductingEquipment ? ConductingEquipment
  {
    get => GetAssoc1To1<ConductingEquipment> (nameof (ConductingEquipment));
    set => SetAssoc1To1 (nameof (ConductingEquipment), value);
  }
  /// <summary>
  /// The connectivity node to which this terminal connects with zero impedance.
  /// </summary>
  public ConnectivityNode ? ConnectivityNode
  {
    get => GetAssoc1To1<ConnectivityNode> (nameof (ConnectivityNode));
    set => SetAssoc1To1 (nameof (ConnectivityNode), value);
  }

  /// <summary>
  /// The auxiliary equipment connected to the terminal.
  /// </summary>
  public AuxiliaryEquipment[] AuxiliaryEquipment => GetAssoc1ToM<
      AuxiliaryEquipment> (nameof (AuxiliaryEquipment));
  public void
  AddToAuxiliaryEquipment (AuxiliaryEquipment assocObject) => AddAssoc1ToM (
      nameof (AuxiliaryEquipment), assocObject);
  public void RemoveFromAuxiliaryEquipment (
      AuxiliaryEquipment
          assocObject) => RemoveAssoc1ToM (nameof (AuxiliaryEquipment),
                                           assocObject);
  public void RemoveAllFromAuxiliaryEquipment () => RemoveAllAssocs1ToM (
      nameof (AuxiliaryEquipment));
}

/// <summary>
/// A base class for all objects that may contain connectivity nodes or
/// topological nodes.
/// </summary>
[CimClass (ClassUri)]
public partial class ConnectivityNodeContainer
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : PowerSystemResource (oid, metaClass)
{
  public new const string ClassUri
      = "http://iec.ch/TC57/CIM100#ConnectivityNodeContainer";

  /// <summary>
  /// Connectivity nodes which belong to this connectivity node container.
  /// </summary>
  public ConnectivityNode[] ConnectivityNodes => GetAssoc1ToM<
      ConnectivityNode> (nameof (ConnectivityNodes));
  public void AddToConnectivityNodes (
      ConnectivityNode assocObject) => AddAssoc1ToM (nameof (ConnectivityNodes),
                                                     assocObject);
  public void RemoveFromConnectivityNodes (
      ConnectivityNode
          assocObject) => RemoveAssoc1ToM (nameof (ConnectivityNodes),
                                           assocObject);
  public void RemoveAllFromConnectivityNodes () => RemoveAllAssocs1ToM (
      nameof (ConnectivityNodes));
}

/// <summary>
/// A modelling construct to provide a root class for containing equipment.
/// </summary>
[CimClass (ClassUri)]
public partial class EquipmentContainer
    (IOIDDescriptor oid, ICimMetaClass metaClass)
    : ConnectivityNodeContainer (oid, metaClass)
{
    public new const string ClassUri
        = "http://iec.ch/TC57/CIM100#EquipmentContainer";

    /// <summary>
    /// The additonal contained equipment.  The equipment belong to the equipment
    /// container. The equipment is contained in another equipment container, but
    /// also grouped with this equipment container.  Examples include when a
    /// switch contained in a substation is also desired to be grouped with a line
    /// contianer or when a switch is included in a secondary substation and also
    /// grouped in a feeder.
    /// </summary>
    public Equipment[] AdditionalGroupedEquipment => GetAssoc1ToM<Equipment> (
        nameof (AdditionalGroupedEquipment));
    public void
        AddToAdditionalGroupedEquipment (Equipment assocObject) => AddAssoc1ToM (
        nameof (AdditionalGroupedEquipment), assocObject);
    public void RemoveFromAdditionalGroupedEquipment (
        Equipment
            assocObject) => RemoveAssoc1ToM (nameof (AdditionalGroupedEquipment),
        assocObject);
    public void
        RemoveAllFromAdditionalGroupedEquipment () => RemoveAllAssocs1ToM (
        nameof (AdditionalGroupedEquipment));
    /// <summary>
    /// Contained equipment.
    /// </summary>
    public Equipment[] Equipments => GetAssoc1ToM<Equipment> (
        nameof (Equipments));
    public void AddToEquipments (Equipment assocObject) => AddAssoc1ToM (
        nameof (Equipments), assocObject);
    public void RemoveFromEquipments (Equipment assocObject) => RemoveAssoc1ToM (
        nameof (Equipments), assocObject);
    public void RemoveAllFromEquipments () => RemoveAllAssocs1ToM (
        nameof (Equipments));
}

/// <summary>
/// A collection of equipment for purposes other than generation or utilization,
/// through which electric energy in bulk is passed for the purposes of
/// switching or modifying its characteristics.
/// </summary>
[CimClass (ClassUri)]
public partial class Substation
    (IOIDDescriptor oid, ICimMetaClass metaClass)
    : EquipmentContainer (oid, metaClass)
{
    public new const string ClassUri = "http://iec.ch/TC57/CIM100#Substation";
  
    /// <summary>
    /// The voltage levels within this substation.
    /// </summary>
    public VoltageLevel[] VoltageLevels => GetAssoc1ToM<VoltageLevel> (
        nameof (VoltageLevels));
    public void AddToVoltageLevels (VoltageLevel assocObject) => AddAssoc1ToM (
        nameof (VoltageLevels), assocObject);
    public void RemoveFromVoltageLevels (
        VoltageLevel assocObject) => RemoveAssoc1ToM (nameof (VoltageLevels),
        assocObject);
    public void RemoveAllFromVoltageLevels () => RemoveAllAssocs1ToM (
        nameof (VoltageLevels));
}

/// <summary>
/// A collection of equipment at one common system voltage forming a switchgear.
/// The equipment typically consists of breakers, busbars, instrumentation,
/// control, regulation and protection devices as well as assemblies of all
/// these.
/// </summary>
[CimClass (ClassUri)]
public partial class VoltageLevel
    (IOIDDescriptor oid, ICimMetaClass metaClass)
    : EquipmentContainer (oid, metaClass)
{
    public new const string ClassUri = "http://iec.ch/TC57/CIM100#VoltageLevel";

    /// <summary>
    /// The base voltage used for all equipment within the voltage level.
    /// </summary>
    public BaseVoltage ? BaseVoltage
    {
        get => GetAssoc1To1<BaseVoltage> (nameof (BaseVoltage));
        set => SetAssoc1To1 (nameof (BaseVoltage), value);
    }
    /// <summary>
    /// The substation of the voltage level.
    /// </summary>
    public Substation ? Substation
    {
        get => GetAssoc1To1<Substation> (nameof (Substation));
        set => SetAssoc1To1 (nameof (Substation), value);
    }

    /// <summary>
    /// The bays within this voltage level.
    /// </summary>
    public Bay[] Bays => GetAssoc1ToM<Bay> (nameof (Bays));
    public void AddToBays (Bay assocObject) => AddAssoc1ToM (nameof (Bays),
        assocObject);
    public void RemoveFromBays (Bay assocObject) => RemoveAssoc1ToM (
        nameof (Bays), assocObject);
    public void RemoveAllFromBays () => RemoveAllAssocs1ToM (nameof (Bays));
}

/// <summary>
/// Tangible resource of the utility, including power system equipment, various
/// end devices, cabinets, buildings, etc. For electrical network equipment, the
/// role of the asset is defined through PowerSystemResource and its subclasses,
/// defined mainly in the Wires model (refer to IEC61970-301 and model package
/// IEC61970::Wires). Asset description places emphasis on the physical
/// characteristics of the equipment fulfilling that role.
/// </summary>
[CimClass (ClassUri)]
public partial class Asset
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : IdentifiedObject (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#Asset";
  
  /// <summary>
  /// In use dates for this asset.
  /// </summary>
  public InUseDate ? inUseDate
  {
    get => GetAttribute < InUseDate ? > (nameof (inUseDate));
    set => SetAttribute (nameof (inUseDate), value);
  }
  
  /// <summary>
  /// Lot number for this asset. Even for the same model and version number,
  /// many assets are manufactured in lots.
  /// </summary>
  public string ? lotNumber
  {
    get => GetAttribute < string ? > (nameof (lotNumber));
    set => SetAttribute (nameof (lotNumber), value);
  }
  /// <summary>
  /// Position of asset or asset component. May often be in relation to other
  /// assets or components.
  /// </summary>
  public string ? position
  {
    get => GetAttribute < string ? > (nameof (position));
    set => SetAttribute (nameof (position), value);
  }
  /// <summary>
  /// Serial number of this asset.
  /// </summary>
  public string ? serialNumber
  {
    get => GetAttribute < string ? > (nameof (serialNumber));
    set => SetAttribute (nameof (serialNumber), value);
  }
  /// <summary>
  /// Utility-specific classification of Asset and its subtypes, according to
  /// their corporate standards, practices, and existing IT systems (e.g., for
  /// management of assets, maintenance, work, outage, customers, etc.).
  /// </summary>
  public string ? type
  {
    get => GetAttribute < string ? > (nameof (type));
    set => SetAttribute (nameof (type), value);
  }
  /// <summary>
  /// Uniquely tracked commodity (UTC) number.
  /// </summary>
  public string ? utcNumber
  {
    get => GetAttribute < string ? > (nameof (utcNumber));
    set => SetAttribute (nameof (utcNumber), value);
  }
  /// <summary>
  ///
  /// </summary>
  public string ? baselineCondition
  {
    get => GetAttribute < string ? > (nameof (baselineCondition));
    set => SetAttribute (nameof (baselineCondition), value);
  }
  /// <summary>
  ///
  /// </summary>
  public float ? baselineLossOfLife
  {
    get => GetAttribute < float ? > (nameof (baselineLossOfLife));
    set => SetAttribute (nameof (baselineLossOfLife), value);
  }
  
  /// <summary>
  /// All power system resources used to electrically model this asset. For
  /// example, transformer asset is electrically modelled with a transformer and
  /// its windings and tap changer.
  /// </summary>
  public PowerSystemResource[] PowerSystemResources => GetAssoc1ToM<
      PowerSystemResource> (nameof (PowerSystemResources));
  public void
  AddToPowerSystemResources (PowerSystemResource assocObject) => AddAssoc1ToM (
      nameof (PowerSystemResources), assocObject);
  public void RemoveFromPowerSystemResources (
      PowerSystemResource
          assocObject) => RemoveAssoc1ToM (nameof (PowerSystemResources),
                                           assocObject);
  public void RemoveAllFromPowerSystemResources () => RemoveAllAssocs1ToM (
      nameof (PowerSystemResources));
}

/// <summary>
/// Dates associated with asset 'in use' status. May have multiple in use dates
/// for this device and a compound type allows a query to return multiple dates.
/// </summary>
[CimClass (ClassUri)]
public partial class InUseDate
(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Resource (oid, metaClass)
{
  public new const string ClassUri = "http://iec.ch/TC57/CIM100#InUseDate";

  /// <summary>
  /// Date asset was most recently put in use.
  /// </summary>
  public DateTime ? inUseDate
  {
    get => GetAttribute < DateTime ? > (nameof (inUseDate));
    set => SetAttribute (nameof (inUseDate), value);
  }
  /// <summary>
  /// Date of most recent asset transition to not ready for use state.
  /// </summary>
  public DateTime ? notReadyForUseDate
  {
    get => GetAttribute < DateTime ? > (nameof (notReadyForUseDate));
    set => SetAttribute (nameof (notReadyForUseDate), value);
  }
  /// <summary>
  /// Date of most recent asset transition to ready for use state.
  /// </summary>
  public DateTime ? readyForUseDate
  {
    get => GetAttribute < DateTime ? > (nameof (readyForUseDate));
    set => SetAttribute (nameof (readyForUseDate), value);
  }
}

/// <summary>
/// An unordered enumeration of phase identifiers.  Allows designation of phases
/// for both transmission and distribution equipment, circuits and loads.   The
/// enumeration, by itself, does not describe how the phases are connected
/// together or connected to ground.  Ground is not explicitly denoted as a
/// phase.Residential and small commercial loads are often served from
/// single-phase, or split-phase, secondary circuits. For the example of s12N,
/// phases 1 and 2 refer to hot wires that are 180 degrees out of phase, while N
/// refers to the neutral wire. Through single-phase transformer connections,
/// these secondary circuits may be served from one or two of the primary phases
/// A, B, and C. For three-phase loads, use the A, B, C phase codes instead of
/// s12N.The integer values are from IEC 61968-9 to support revenue metering
/// applications.
/// </summary>
[CimClass ("http://iec.ch/TC57/CIM100#PhaseCode")]
public enum PhaseCode
{
  /// <summary>
  /// Phase A.
  /// </summary>
  A, /// <summary>
  /// Phases A and B.
  /// </summary>
  AB, /// <summary>
  /// Phases A, B, and C.
  /// </summary>
  ABC, /// <summary>
  /// Phases A, B, C, and N.
  /// </summary>
  ABCN, /// <summary>
  /// Phases A, B, and neutral.
  /// </summary>
  ABN, /// <summary>
  /// Phases A and C.
  /// </summary>
  AC, /// <summary>
  /// Phases A, C and neutral.
  /// </summary>
  ACN, /// <summary>
  /// Phases A and neutral.
  /// </summary>
  AN, /// <summary>
  /// Phase B.
  /// </summary>
  B, /// <summary>
  /// Phases B and C.
  /// </summary>
  BC, /// <summary>
  /// Phases B, C, and neutral.
  /// </summary>
  BCN, /// <summary>
  /// Phases B and neutral.
  /// </summary>
  BN, /// <summary>
  /// Phase C.
  /// </summary>
  C, /// <summary>
  /// Phases C and neutral.
  /// </summary>
  CN, /// <summary>
  /// Neutral phase.
  /// </summary>
  N, /// <summary>
  /// Unknown non-neutral phase.
  /// </summary>
  X, /// <summary>
  /// Unknown non-neutral phase plus neutral.
  /// </summary>
  XN, /// <summary>
  /// Two unknown non-neutral phases.
  /// </summary>
  XY, /// <summary>
  /// Two unknown non-neutral phases plus neutral.
  /// </summary>
  XYN, /// <summary>
  /// No phases specified.
  /// </summary>
  none,
}

/// <summary>
/// The construction kind of the potential transformer.
/// </summary>
[CimClass ("http://iec.ch/TC57/CIM100#PotentialTransformerKind")]
public enum PotentialTransformerKind
{
  /// <summary>
  /// The potential transformer is using capacitive coupling to create secondary
  /// voltage.
  /// </summary>
  capacitiveCoupling, /// <summary>
  /// The potential transformer is using induction coils to create secondary
  /// voltage.
  /// </summary>
  inductive,
}