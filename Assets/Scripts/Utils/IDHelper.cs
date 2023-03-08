using System.Runtime.InteropServices;

namespace Utils
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct InstanceIdStruct
	{
		private readonly ulong Value; // 46bit
		public readonly int AppId; // 18bit

		public long ToLong()
		{
			ulong result = 0;
			result |= (uint)this.AppId;
			result |= this.Value << 18;
			return (long)result;
		}

		public InstanceIdStruct(long id)
		{
			ulong result = (ulong)id;
			AppId = (int)(result & 0x03ffff);
			result >>= 18;
			Value = result;
		}

		public InstanceIdStruct(int appId, ulong value)
		{
			AppId = appId;
			Value = value;
		}

		public override string ToString()
		{
			return $"appId: {this.AppId}, value: {this.Value}";
		}
	}
	public static class IDGenerator
	{
		private static int appId;

		public static int AppId
		{
			set => appId = value;
		}


		private static ulong instanceIdValue = 0;
		public static long GenerateInstanceId()
		{
			var instanceIdStruct = new InstanceIdStruct(appId, ++instanceIdValue);
			return instanceIdStruct.ToLong();
		}

		public static int GetAppId(long v)
		{
			return new InstanceIdStruct(v).AppId;
		}
	}
}