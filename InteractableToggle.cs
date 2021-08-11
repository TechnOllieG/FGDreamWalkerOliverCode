namespace GP2_Team7.Objects
{
	[InteractOn(Interaction.KeyDown)]
	public class InteractableToggle : Interactable
	{
		public bool poweredAtStart = false;

		protected override void Awake()
		{
			base.Awake();
			
			if(poweredAtStart)
				base.Interact();
		}
	}
}