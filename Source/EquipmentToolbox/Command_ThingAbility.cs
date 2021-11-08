using UnityEngine;
using Verse;

namespace EquipmentToolbox
{
	public class Command_ThingAbility : Command_VerbTarget
	{
		public Command_ThingAbility(CompThingAbility comp)
		{
			this.comp = comp;
		}

		public override string TopRightLabel
		{
			get
			{
				if (comp.AmmoDef == null && !comp.Props.displayInfiniteAmmoOnGizmo) return null;
				return comp.LabelRemaining;
			}
		}
		public override Color IconDrawColor
		{
			get
			{
				Color? color = this.overrideColor;
				if (color == null)
				{
					return base.IconDrawColor;
				}
				return color.GetValueOrDefault();
			}
		}

		public override void GizmoUpdateOnMouseover()
		{
			verb.DrawHighlight(LocalTargetInfo.Invalid);
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
		}

		public override bool GroupsWith(Gizmo other)
		{
			return false;
		}

		private readonly CompThingAbility comp;
		public Color? overrideColor;
	}
}
