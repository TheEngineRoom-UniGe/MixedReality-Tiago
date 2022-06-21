//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.TiagoUnity
{
    [Serializable]
    public class PickPlaceServiceResponse : Message
    {
        public const string k_RosMessageName = "tiago_unity/PickPlaceService";
        public override string RosMessageName => k_RosMessageName;

        public PlannedTrajectoryMsg arm_trajectory;

        public PickPlaceServiceResponse()
        {
            this.arm_trajectory = new PlannedTrajectoryMsg();
        }

        public PickPlaceServiceResponse(PlannedTrajectoryMsg arm_trajectory)
        {
            this.arm_trajectory = arm_trajectory;
        }

        public static PickPlaceServiceResponse Deserialize(MessageDeserializer deserializer) => new PickPlaceServiceResponse(deserializer);

        private PickPlaceServiceResponse(MessageDeserializer deserializer)
        {
            this.arm_trajectory = PlannedTrajectoryMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.arm_trajectory);
        }

        public override string ToString()
        {
            return "PickPlaceServiceResponse: " +
            "\narm_trajectory: " + arm_trajectory.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Response);
        }
    }
}