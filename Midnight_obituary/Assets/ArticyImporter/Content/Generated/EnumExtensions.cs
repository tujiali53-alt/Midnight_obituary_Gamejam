namespace Articy.ArticyProject
{
	public static class EnumExtensionMethods
	{
		public static string GetDisplayName(this sex asex)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("sex").GetEnumValue(((int)(asex))).DisplayName;
		}

		public static string GetDisplayName(this perceiving aperceiving)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("perceiving").GetEnumValue(((int)(aperceiving))).DisplayName;
		}

		public static string GetDisplayName(this decision adecision)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("decision").GetEnumValue(((int)(adecision))).DisplayName;
		}

		public static string GetDisplayName(this ShapeType aShapeType)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("ShapeType").GetEnumValue(((int)(aShapeType))).DisplayName;
		}

		public static string GetDisplayName(this SelectabilityModes aSelectabilityModes)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("SelectabilityModes").GetEnumValue(((int)(aSelectabilityModes))).DisplayName;
		}

		public static string GetDisplayName(this VisibilityModes aVisibilityModes)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("VisibilityModes").GetEnumValue(((int)(aVisibilityModes))).DisplayName;
		}

		public static string GetDisplayName(this OutlineStyle aOutlineStyle)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("OutlineStyle").GetEnumValue(((int)(aOutlineStyle))).DisplayName;
		}

		public static string GetDisplayName(this PathCaps aPathCaps)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("PathCaps").GetEnumValue(((int)(aPathCaps))).DisplayName;
		}

		public static string GetDisplayName(this LocationAnchorSize aLocationAnchorSize)
		{
			return Articy.Unity.ArticyTypeSystem.GetArticyType("LocationAnchorSize").GetEnumValue(((int)(aLocationAnchorSize))).DisplayName;
		}

	}
}

