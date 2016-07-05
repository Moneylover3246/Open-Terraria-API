﻿using NDesk.Options;
using OTAPI.Patcher.Engine.Extensions;
using OTAPI.Patcher.Engine.Modification;
using System.Linq;

namespace OTAPI.Patcher.Engine.Modifications.Hooks.Npc
{
	public class NetDefaults : ModificationBase
	{
		public override string Description => "Hooking Npc.NetDefaults(int)...";
		public override void Run(OptionSet options)
		{
			var vanilla = SourceDefinition.Type("Terraria.NPC").Methods.Single(
				x => x.Name == "netDefaults"
				&& x.Parameters.First().ParameterType == SourceDefinition.MainModule.TypeSystem.Int32
			);


			var cbkBegin = ModificationDefinition.Type("OTAPI.Core.Callbacks.Terraria.Npc").Method("NetDefaultsBegin", parameters: vanilla.Parameters);
			var cbkEnd = ModificationDefinition.Type("OTAPI.Core.Callbacks.Terraria.Npc").Method("NetDefaultsEnd", parameters: vanilla.Parameters);

			vanilla.Wrap(cbkBegin, cbkEnd, true);
		}
	}
}