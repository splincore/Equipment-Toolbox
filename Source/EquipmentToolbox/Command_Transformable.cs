using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EquipmentToolbox
{
    public class Command_Transformable : Command_Action
    {
        public Command_Transformable(CompTransformable comp)
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

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
		}

		public override bool GroupsWith(Gizmo other)
		{
			return false;
		}

		private readonly CompTransformable comp;
        public Color? overrideColor;
    }
}
