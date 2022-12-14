//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.MoveBase
{
    [Serializable]
    public class MoveBaseActionMsg : Message
    {
        public const string k_RosMessageName = "move_base_msgs/MoveBaseAction";
        public override string RosMessageName => k_RosMessageName;

        //  ====== DO NOT MODIFY! AUTOGENERATED FROM AN ACTION DEFINITION ======
        public MoveBaseActionGoalMsg action_goal;
        public MoveBaseActionResultMsg action_result;
        public MoveBaseActionFeedbackMsg action_feedback;

        public MoveBaseActionMsg()
        {
            this.action_goal = new MoveBaseActionGoalMsg();
            this.action_result = new MoveBaseActionResultMsg();
            this.action_feedback = new MoveBaseActionFeedbackMsg();
        }

        public MoveBaseActionMsg(MoveBaseActionGoalMsg action_goal, MoveBaseActionResultMsg action_result, MoveBaseActionFeedbackMsg action_feedback)
        {
            this.action_goal = action_goal;
            this.action_result = action_result;
            this.action_feedback = action_feedback;
        }

        public static MoveBaseActionMsg Deserialize(MessageDeserializer deserializer) => new MoveBaseActionMsg(deserializer);

        private MoveBaseActionMsg(MessageDeserializer deserializer)
        {
            this.action_goal = MoveBaseActionGoalMsg.Deserialize(deserializer);
            this.action_result = MoveBaseActionResultMsg.Deserialize(deserializer);
            this.action_feedback = MoveBaseActionFeedbackMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

        public override string ToString()
        {
            return "MoveBaseActionMsg: " +
            "\naction_goal: " + action_goal.ToString() +
            "\naction_result: " + action_result.ToString() +
            "\naction_feedback: " + action_feedback.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
