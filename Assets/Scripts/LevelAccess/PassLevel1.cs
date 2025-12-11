using UnityEngine;
using UnityEngine.SceneManagement;

public class PassLevel1 : LevelGoalBase
{
    protected override string NextSceneName => "level 2";
}
