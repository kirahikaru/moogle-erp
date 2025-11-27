using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TechAdminERP.Static;

[ExcludeFromCodeCoverage]
public static class PruColors
{
	public static class Gray
	{
		//public static string Lightest => "#F2F2F2";
		public static string Lighter => "#F2F2F2";
		public static string Light => "#D9D9D9";
		public static string Dark => "#BFBFBF";
		public static string Darker => "#A6A6A6";
		public static string Darkest => "#7F7F7F";
	}

	public static class DarkGray
	{
		public static string Default => "#D0D0CE";
		//public static string Lightest => "#F2F2F2";
		public static string Lighter => "#BCBCB9";
		public static string Light => "#9D9D99";
		public static string Dark => "#6A6A65";
		public static string Darker => "#353533";
		public static string Darkest => "#151514";
	}

	public static class PruGray
	{
		public static string Default => "#687379";
		//public static string Lightest => "#F2F2F2";
		public static string Lighter => "#E0E3E5";
		public static string Light => "#C2C7CA";
		public static string Dark => "#A3ABB0";
		public static string Darker => "#4E565B";
		public static string Darkest => "#343A3C";
	}

	public static class PruRed
	{
		public static string Default => "#ED1B2E";
		//public static string Lightest => "#F2F2F2";
		public static string Lighter => "#FBD1D5";
		public static string Light => "#F8A4AB";
		public static string Dark => "#F47682";
		public static string Darker => "#B80E1E";
		public static string Darkest => "#7A0A14";
	}

	public static class PruLightRed
	{
		public static string Default = "#F37682";
		//public static string Lightest => "#F2F2F2";
		public static string Lighter => "#FDE4E6";
		public static string Light => "#FAC8CD";
		public static string Dark => "#F8ADB4";
		public static string Darker => "#EC2336";
		public static string Darkest => "#A60F1D";
	}

	public static class Navy
	{
		public static string Default = "#1B365D";
		//public static string Lightest => "#F2F2F2";
		public static string Lighter => "#C3D4ED";
		public static string Light => "#86A9DC";
		public static string Dark => "#4A7ECA";
		public static string Darker => "#142846";
		public static string Darkest => "#0E1B2E";
	}

	public static class Teal
	{
		public static string Default = "#5CB8B2";
		//public static string Lightest => "#F2F2F2";
		public static string Lighter => "#DEF1F0";
		public static string Light => "#BEE3E0";
		public static string Dark => "#9DD4D1";
		public static string Darker => "#3F908B";
		public static string Darkest => "#2A605D";
	}
}