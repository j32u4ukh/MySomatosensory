using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ETLab
{
    public class IntegerEvent : UnityEvent<int> { }
    public class StringEvent : UnityEvent<string> { }

    public class PoseEvent : UnityEvent<Pose> { }

}
