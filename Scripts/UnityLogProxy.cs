using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GRPExplorerLib.Logging;

public class UnityLogInterface : IGRPExplorerLibLogInterface
{
    public void Debug(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    public void Error(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    public void Info(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }
}
