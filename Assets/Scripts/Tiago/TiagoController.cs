using System.Collections;
using UnityEngine;

using RosMessageTypes.TiagoHoloDt;
using System.Linq;

using Point = RosMessageTypes.Geometry.PointMsg;
using V3 = RosMessageTypes.Geometry.Vector3Msg;
using Bool = RosMessageTypes.Std.BoolMsg;

public class TiagoController : MonoBehaviour
{
    // Robot object
    private GameObject tiago;
    private GameObject base_link;

    // Arrays of ArticulationBodies for controlling arms
    private ArticulationBody[] leftArmArticulationBodies;
    private ArticulationBody[] leftGripper;
    private ArticulationBody[] rightArmArticulationBodies;
    private ArticulationBody[] rightGripper;
    private ArticulationBody torsoArticulationBody;
    private int[] leftArmIndices = { 0, 1, 2, 3, 4, 5, 6 }; 
    private int[] rightArmIndices = { 7, 8, 9, 10, 11, 12, 13 };
    private int torsoIndex = 20;

    // Variables for trajectory execution
    private string lastArmActionPlanned = "";
    private PlannedActionMsg lastActionPlanned;
    private int steps;
    private float jointAssignmentWait = 0.01f;
    public bool busy = false;

    public void Initialize(GameObject robot, GameObject baseLink, int steps)
    {
        tiago = robot;
        base_link = baseLink;

        this.steps = steps;

        // Get reference to robot and go to home position
        GetRobotReference();
    }

    // Initial routine to obtain reference to all objects in the scene needed to drive the motion of the holographic robot
    private void GetRobotReference()
    {
        string base_link = "base_footprint/base_link";
        string torso_link = base_link + "/torso_fixed_link/torso_lift_link";
        torsoArticulationBody = tiago.transform.Find(torso_link).GetComponent<ArticulationBody>();

        leftArmArticulationBodies = new ArticulationBody[7];
        string side = "left";
        string arm_1_link = torso_link + "/arm_" + side  + "_1_link";
        leftArmArticulationBodies[0] = tiago.transform.Find(arm_1_link).GetComponent<ArticulationBody>();

        string arm_2_link = arm_1_link + "/arm_" + side + "_2_link";
        leftArmArticulationBodies[1] = tiago.transform.Find(arm_2_link).GetComponent<ArticulationBody>();

        string arm_3_link = arm_2_link + "/arm_" + side + "_3_link";
        leftArmArticulationBodies[2] = tiago.transform.Find(arm_3_link).GetComponent<ArticulationBody>();

        string arm_4_link = arm_3_link + "/arm_" + side + "_4_link";
        leftArmArticulationBodies[3] = tiago.transform.Find(arm_4_link).GetComponent<ArticulationBody>();

        string arm_5_link = arm_4_link + "/arm_" + side + "_5_link";
        leftArmArticulationBodies[4] = tiago.transform.Find(arm_5_link).GetComponent<ArticulationBody>();

        string arm_6_link = arm_5_link + "/arm_" + side + "_6_link";
        leftArmArticulationBodies[5] = tiago.transform.Find(arm_6_link).GetComponent<ArticulationBody>();

        string arm_7_link = arm_6_link + "/arm_" + side + "_7_link";
        leftArmArticulationBodies[6] = tiago.transform.Find(arm_7_link).GetComponent<ArticulationBody>();

        string gripper_left_link = arm_7_link + "/arm_" + side + "_tool_link/gripper_" + side + "_link";
        string left_gripper_left_finger = gripper_left_link + "/gripper_" + side + "_left_finger_link";
        string left_gripper_right_finger = gripper_left_link + "/gripper_" + side + "_right_finger_link";

        leftGripper = new ArticulationBody[2];
        leftGripper[0] = tiago.transform.Find(left_gripper_left_finger).GetComponent<ArticulationBody>();
        leftGripper[1] = tiago.transform.Find(left_gripper_right_finger).GetComponent<ArticulationBody>();

        rightArmArticulationBodies = new ArticulationBody[7];
        side = "right";
        arm_1_link = torso_link + "/arm_" + side + "_1_link";
        rightArmArticulationBodies[0] = tiago.transform.Find(arm_1_link).GetComponent<ArticulationBody>();

        arm_2_link = arm_1_link + "/arm_" + side + "_2_link";
        rightArmArticulationBodies[1] = tiago.transform.Find(arm_2_link).GetComponent<ArticulationBody>();

        arm_3_link = arm_2_link + "/arm_" + side + "_3_link";
        rightArmArticulationBodies[2] = tiago.transform.Find(arm_3_link).GetComponent<ArticulationBody>();

        arm_4_link = arm_3_link + "/arm_" + side + "_4_link";
        rightArmArticulationBodies[3] = tiago.transform.Find(arm_4_link).GetComponent<ArticulationBody>();

        arm_5_link = arm_4_link + "/arm_" + side + "_5_link";
        rightArmArticulationBodies[4] = tiago.transform.Find(arm_5_link).GetComponent<ArticulationBody>();

        arm_6_link = arm_5_link + "/arm_" + side + "_6_link";
        rightArmArticulationBodies[5] = tiago.transform.Find(arm_6_link).GetComponent<ArticulationBody>();

        arm_7_link = arm_6_link + "/arm_" + side + "_7_link";
        rightArmArticulationBodies[6] = tiago.transform.Find(arm_7_link).GetComponent<ArticulationBody>();

        string gripper_right_link = arm_7_link + "/arm_" + side + "_tool_link/wrist_" + side + "_ft_link/wrist_" + side + "_ft_tool_link/gripper_" + side + "_link";
        string right_gripper_left_finger = gripper_right_link + "/gripper_" + side + "_left_finger_link";
        string right_gripper_right_finger = gripper_right_link + "/gripper_" + side + "_right_finger_link";

        rightGripper = new ArticulationBody[2];
        rightGripper[0] = tiago.transform.Find(right_gripper_left_finger).GetComponent<ArticulationBody>();
        rightGripper[1] = tiago.transform.Find(right_gripper_right_finger).GetComponent<ArticulationBody>();

    }

    // Routine to teleport the robot's base_link to a new gobal pose, expressed via position and rotation
    public void TeleportRobot(Vector3 position, Quaternion rotation)
    {
        base_link.GetComponent<ArticulationBody>().TeleportRoot(position, rotation);
    }

    // Routine to teleport the robot's base_link to a relative pose wrt the current one
    public void TeleportRobotPosition(Vector3 position)
    {
        base_link.GetComponent<ArticulationBody>().TeleportRoot(position, base_link.transform.rotation);
    }

    // Methods to close and open the robot's gripper
    public void CloseGripper(ArticulationBody[] gripper)
    {
        var leftDrive = gripper[0].xDrive;
        var rightDrive = gripper[1].xDrive;

        leftDrive.target = 0.02f;
        rightDrive.target = 0.02f;

        gripper[0].xDrive = leftDrive;
        gripper[1].xDrive = rightDrive;
    }

    public void OpenGripper(ArticulationBody[] gripper)
    {
        var leftDrive = gripper[0].xDrive;
        var rightDrive = gripper[1].xDrive;

        // Hardcoded values for open gripper (extracted from Tiago's documentation)
        leftDrive.target = 0.045f;
        rightDrive.target = 0.045f;

        gripper[0].xDrive = leftDrive;
        gripper[1].xDrive = rightDrive;
    }

    /* ---------------------
     * Utility methods to drive the robot (arms and torso) to their initial rest configurations when the application starts
     * Can be set up in order to copy the real robot's configuration if connected
     */

    public void JointStateServiceResponse(JointStateServiceResponse response)
    {
        if (response != null && response.robot_state.name.Length > 2)
        {
            MoveToRestPosition(response);
        }
    }

    public void MoveToRestPosition(JointStateServiceResponse response)
    {
        var left_arm_joint_values = new float[7];
        var right_arm_joint_values = new float[7];

        // Extract the joint values for both arms from the service response message, using the hard-coded indices known from Tiago's /joint_states topic
        for(int i=0; i<leftArmIndices.Length; i++)
        {
            left_arm_joint_values[i] = Mathf.Rad2Deg * (float)response.robot_state.position[leftArmIndices[i]];
            right_arm_joint_values[i] = Mathf.Rad2Deg * (float)response.robot_state.position[rightArmIndices[i]];
        }

        // Call the internal routines to drive both arms and torso to their target joint values
        StartCoroutine(MoveArmToRestRoutine(leftArmArticulationBodies, left_arm_joint_values, leftGripper));
        StartCoroutine(MoveArmToRestRoutine(rightArmArticulationBodies, right_arm_joint_values, rightGripper));
        StartCoroutine(MoveTorsoToRestRoutine((float)response.robot_state.position[torsoIndex]));
    }
    
    /* Internal routine for moving one robot arm to a given joint configuration
     * Params:
     *      - The arm articulation body, represented as an array of ArticulationBody objects;
     *      - The target values for the joints of the arm, given as array of floats and expressed in degrees;
     *      - The gripper articulation body, so that the respective gripper can be set to default configuration (open).
     */
    private IEnumerator MoveArmToRestRoutine(ArticulationBody[] armArticulationBody, float[] target_config, ArticulationBody[] gripperArticulationBody)
    {
        float[] lastJointState = { 0, 0, 0, 0, 0, 0, 0 };
        var steps = 200;

        for (int i = 0; i <= steps; i++)
        {
            for (int joint = 0; joint < armArticulationBody.Length; joint++)
            {
                var joint1XDrive = armArticulationBody[joint].xDrive;
                joint1XDrive.target = lastJointState[joint] + (target_config[joint] - lastJointState[joint]) * (float)(1.0f / steps) * (float)i;
                armArticulationBody[joint].xDrive = joint1XDrive;
            }

            yield return new WaitForSeconds(jointAssignmentWait);
        }
        OpenGripper(gripperArticulationBody);
    }

    private IEnumerator MoveTorsoToRestRoutine(float target)
    {
        var steps = 100;
        var torsoDrive = torsoArticulationBody.xDrive;
        var lastTorsoValue = torsoDrive.target;
        for (int i = 0; i <= steps; i++)
        {            
            torsoDrive.target = lastTorsoValue + (target - lastTorsoValue) * (float)(1.0f / steps) * (float)i;
            torsoArticulationBody.xDrive = torsoDrive;
            lastTorsoValue = torsoDrive.target;
            yield return new WaitForSeconds(jointAssignmentWait);
        }
    }

    // ---------------------

    /* Utility method to extract the joint configuration as an array of double from a given array of ArticulationObject */
    double[] GetCurrentJointStates(ArticulationBody[] arm)
    {
        var jointAngles = new double[8];
        jointAngles[0] = torsoArticulationBody.xDrive.target;
        for (int i = 1; i < 8; i++)
        {
            // Convert from degrees to radiants as ROS expects rads for planning
            jointAngles[i] = Mathf.Deg2Rad * arm[i-1].xDrive.target;
        }
        
        return jointAngles;
    }

    /* Utility method to contruct a planning request object accepted by the ROS service
     * Params:
     *      - arm: the arm for which a plan is sought;
     *      - actionType: the type of action for which a plan is sought (pick_place, handover ...)
     *      - pickPos: a 3D point corresponding to the position of the object to pick (for pick_place actions) / handover position;
     *      - placePos: a 3D point defining the location to place the object / useless for handover actions
     *      - graspDir: a 3D vector defining the direction of approach and grasping for the robot. Two possibilities are currently contemplated:
     *              1. Frontal (forward) grasp = (1,0,0);
     *              2. Vertical (descending) grasp = (0,0,-1).
     */
    public ActionServiceRequest PlanningRequest(string arm, string actionType, Point pickPos, Point placePos, V3 graspDir)
    {
        var request = new ActionServiceRequest();

        request.action_type = actionType;
        request.pick_pos = pickPos;
        if(actionType != "handover")
        {
            request.place_pos = placePos;
        }
        var bodies = arm == "left" ? leftArmArticulationBodies : rightArmArticulationBodies;
        // Get current joint configuration for the given arm from the articulation bodies objects
        request.joint_angles = GetCurrentJointStates(bodies);
        request.grasp_direction = graspDir;

        return request;
    }

    /* Public callback method accessible from external Interface, called when the planning service returns
     * If an action is returned, instantiate the coroutine to render it as holographic animation 
     */
    public void ActionPlanningServiceResponse(PlannedActionWithTypeAndArmMsg action)
    {
        // If the planner actually found a solution...
        if (action.planning_result)
        {
            Debug.Log("Trajectory returned.");
         
            lastArmActionPlanned = action.planning_arm;
            lastActionPlanned = action.planned_action;

            StartCoroutine(RenderFullHolographicAction(action));
        }
        else 
            Debug.Log("No trajectory returned from MoveIt.");
    }

    /* Routine to holo-render all trajectories in a given action.
     * Calls the RenderHolographicTrajectory method iteratively on all sub-trajectories compisosing the global action, depending
     * on which type of action is being executed (handover, pick-place ...)
     */
    private IEnumerator RenderFullHolographicAction(PlannedActionWithTypeAndArmMsg action)
    {
        var armArticulationBodies = action.planning_arm == "left" ? leftArmArticulationBodies : rightArmArticulationBodies;
        var gripperArticulationBody = action.planning_arm == "left" ? leftGripper : rightGripper;

        // Pre-grasp trajectory is always rendered
        if(action.action_type != "handover")
        {
            yield return RenderHolographicTrajectory(action.planned_action.pre_grasp_trajectory, armArticulationBodies, this.steps);
        }
        else
            yield return RenderHolographicTrajectory(action.planned_action.pre_grasp_trajectory, armArticulationBodies, 1);

        // Render the following only if pick and place action
        if (action.action_type != "handover")
        {
            yield return RenderHolographicTrajectory(action.planned_action.grasp_trajectory, armArticulationBodies, steps);
            CloseGripper(gripperArticulationBody);
            yield return RenderHolographicTrajectory(action.planned_action.move_trajectory, armArticulationBodies, steps);
            yield return RenderHolographicTrajectory(action.planned_action.place_trajectory, armArticulationBodies, steps);
            OpenGripper(gripperArticulationBody);
            yield return RenderHolographicTrajectory(action.planned_action.return_trajectory, armArticulationBodies, Mathf.RoundToInt(steps / 2));
        }
        // Action complete, controller no more busy
        busy = false;
    }

    /* Internal routine for rendering a single trajectory.
     * Params:
     *      - The trajectory to render;
     *      - The array of articulation bodies that compose the kinematic chain;
     *      - An integer number of steps, which defines the "granularity" of the holographic animation.
     */
    private IEnumerator RenderHolographicTrajectory(RosMessageTypes.Moveit.RobotTrajectoryMsg trajectory, ArticulationBody[] articulationBodies, int steps)
    {
        // Initial joint configuration
        var lastJointState = GetCurrentJointStates(articulationBodies).Select(r => (double)r * Mathf.Rad2Deg).ToArray();

        // For each joint configuration in the given trajectory...
        for (int jointConfigIndex = 0; jointConfigIndex < trajectory.joint_trajectory.points.Length; jointConfigIndex++)
        {
            // Get next joint config and convert from radiant to degree angles (for Unity)
            var jointPositions = trajectory.joint_trajectory.points[jointConfigIndex].positions;
            double[] result = jointPositions.Select(r => (double)r * Mathf.Rad2Deg).ToArray();

            // Get reference to torso drive to control torso al well according to the trajectory being rendered
            var torsoDrive = torsoArticulationBody.xDrive;
            var lastTorsoValue = torsoDrive.target;

            // Steps drive the smoothness of the animation
            for (int i = 0; i <= steps; i++)
            {
                // For each joint in the kinematic chain...
                for (int joint = 0; joint < articulationBodies.Length; joint++)
                {
                    // compute next joint position based on next joint configuration extracted from the trajectory
                    var joint1XDrive = articulationBodies[joint].xDrive;
                    joint1XDrive.target = (float)(lastJointState[joint + 1] + (result[joint + 1] - lastJointState[joint + 1]) * (1.0f / steps) * i);
                    articulationBodies[joint].xDrive = joint1XDrive;
                }

                // Same for torso to achieve next position
                torsoDrive.target = lastTorsoValue + ((float)jointPositions[0] - lastTorsoValue) * (float)(1.0f / steps) * (float)i;
                torsoArticulationBody.xDrive = torsoDrive;

                // Wait few milliseconds to ensure updates to all joints are received
                yield return new WaitForSeconds(jointAssignmentWait);
            }

            lastTorsoValue = torsoDrive.target;
            lastJointState = result;
        }
    }

    // Routine to complete handover, namely robot grasps the item offered by human and goes to transport joint configuration
    public void CompleteHandover()
    {
        var gripper = lastArmActionPlanned == "left" ? leftGripper : rightGripper;
        var articulationBody = lastArmActionPlanned == "left" ? leftArmArticulationBodies : rightArmArticulationBodies;

        CloseGripper(gripper);
        StartCoroutine(RenderHolographicTrajectory(lastActionPlanned.return_trajectory, articulationBody, steps));

    }
}
