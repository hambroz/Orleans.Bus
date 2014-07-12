using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class MockStorageProvider : StateStorageProvider<int>
    {
        public MockStorageProvider()
        {
            Reset();
        }

        public static string ReadStateGrainId
        {
            get { return GetEnv("ReadStateGrainId"); }
            set { SetEnv("ReadStateGrainId", value); }
        }

        public static string ReadStateGrainType
        {
            get { return GetEnv("ReadStateGrainType"); }
            set { SetEnv("ReadStateGrainType", value); }
        }

        public static int ReadStateReturnValue
        {
            get { return int.Parse(GetEnv("ReadStateReturnValue")); }
            set { SetEnv("ReadStateReturnValue", value.ToString()); }
        }

        public static string WriteStateGrainId
        {
            get { return GetEnv("WriteStateGrainId"); }
            set { SetEnv("WriteStateGrainId", value); }
        }

        public static string WriteStateGrainType
        {
            get { return GetEnv("WriteStateGrainType"); }
            set { SetEnv("WriteStateGrainType", value); }
        }

        public static int WriteStatePassedValue
        {
            get { return int.Parse(GetEnv("WriteStatePassedValue")); }
            set { SetEnv("WriteStatePassedValue", value.ToString()); }
        }

        public static string ClearStateGrainId
        {
            get { return GetEnv("ClearStateGrainId"); }
            set { SetEnv("ClearStateGrainId", value); }
        }

        public static string ClearStateGrainType
        {
            get { return GetEnv("ClearStateGrainType"); }
            set { SetEnv("ClearStateGrainType", value); }
        }

        public static int ClearStatePassedValue
        {
            get { return int.Parse(GetEnv("ClearStatePassedValue")); }
            set { SetEnv("ClearStatePassedValue", value.ToString()); }
        }

        public static int DefaultValue
        {
            get { return int.Parse(GetEnv("DefaultValue")); }
            set { SetEnv("DefaultValue", value.ToString()); }
        }

        static string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        static void SetEnv(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        public static void Reset()
        {
            ReadStateReturnValue = 
                WriteStatePassedValue =
                ClearStatePassedValue = -1;

            ReadStateGrainId =
                WriteStateGrainId = 
                ClearStateGrainId = "";

            ReadStateGrainType =
                WriteStateGrainType =
                ClearStateGrainType = "";
        }

        public override Task Init(Dictionary<string, string> properties)
        {
            DefaultValue = int.Parse(properties["DefaultValue"]);
            ReadStateReturnValue = DefaultValue;
            return TaskDone.Done;
        }

        public override Task<int> ReadStateAsync(string grainId, string grainType)
        {
            ReadStateGrainId = grainId;
            ReadStateGrainType = grainType;
            return Task.FromResult(ReadStateReturnValue);
        }

        public override Task WriteStateAsync(string grainId, string grainType, int grainState)
        {
            WriteStateGrainId = grainId;
            WriteStateGrainType = grainType;
            WriteStatePassedValue = grainState;
            return TaskDone.Done;
        }

        public override Task ClearStateAsync(string grainId, string grainType, int grainState)
        {
            ClearStateGrainId = grainId;
            ClearStateGrainType = grainType;
            ClearStatePassedValue = grainState;
            return TaskDone.Done;
        }
    }
}