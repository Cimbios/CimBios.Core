var tree = "\n";

var treeStack = new Stack<IModelObject>(GetObjects<Substation>());

var visited = new HashSet<IModelObject>();

while (treeStack.Any())
{
	var next = treeStack.Pop();
	
	if (visited.Contains(next))
	{
		visited.Remove(next);
		continue;
	}
	
	var title = next.OID.ToString();
	
	if (next is IdentifiedObject io) title += $" : {io.name}";
	
	treeStack.Push(next);
	
	if (next is ConnectivityNodeContainer cnc)
	{
		foreach (var cn in cnc.ConnectivityNodes) treeStack.Push(cn);
	}
	
	if (next is EquipmentContainer eqc)
	{
		foreach (var eq in eqc.ConnectivityNodes) treeStack.Push(eq);
	}
	
	if (next is ConductingEquipment ce)
	{
		foreach (var t in ce.Terminals) treeStack.Push(t);
	}
	
	if (next is ConnectivityNode connectivityNode)
	{
		foreach (var t in connectivityNode.Terminals) treeStack.Push(t);
	}
	
	if (next is PowerSystemResource psr)
	{
		foreach (var asset in psr.Assets) treeStack.Push(asset);
	}
	
	if (next is Substation substation)
	{
		foreach (var vl in substation.VoltageLevels) treeStack.Push(vl);
	}
	
	if (next is VoltageLevel voltageLevel)
	{
		foreach (var b in voltageLevel.Bays) treeStack.Push(b);
	}
	
	if (next is Bay bay)
	{
		foreach (var cn in bay.ConnectivityNodes) treeStack.Push(cn);
		foreach (var eq in bay.Equipments) treeStack.Push(eq);
	}
	
	if (next is PowerTransformer powerTransformer)
	{
		foreach (var pte in powerTransformer.PowerTransformerEnd) treeStack.Push(pte);
		foreach (var ptt in powerTransformer.TransformerTanks) treeStack.Push(ptt);
	}
	
	for (int s = 0; s < visited.Count(); ++s) tree += "\t";
	tree += $"{title} ({next.MetaClass.ShortName})\n";
	
	visited.Add(next);
}

return tree;