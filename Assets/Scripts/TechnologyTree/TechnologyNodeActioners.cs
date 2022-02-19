using System.Collections.Generic;
using UnityEngine;

public class TechnologyNodeActioners
{

    private static Dictionary<string, float> _MULTIPLIERS =
        new Dictionary<string, float>();

    private static Dictionary<string, System.Action> _ACTIONERS =
        new Dictionary<string, System.Action>()
    {
        { "attack_booster", () =>
        {
            // store multiplier for future units
            float multiplier = 2f;
            _MULTIPLIERS["attack_booster"] = multiplier;

            // re-assign my current units attack, if there are any
            if (GameManager.instance == null) return;

            int myPlayerId =
                GameManager.instance.gamePlayersParameters.myPlayerId;
            if (Unit.UNITS_BY_OWNER != null)
                foreach (Unit unit in Unit.UNITS_BY_OWNER[myPlayerId])
                    unit.SetAttackDamage((int) (unit.AttackDamage * multiplier));
        } },
        { "attack_booster_2", () =>
        {
            // store multiplier for future units
            float multiplier = 4f;
            _MULTIPLIERS["attack_booster"] = multiplier;

            // re-assign my current units attack, if there are any
            if (GameManager.instance == null) return;

            int myPlayerId =
                GameManager.instance.gamePlayersParameters.myPlayerId;
            if (Unit.UNITS_BY_OWNER != null)
                foreach (Unit unit in Unit.UNITS_BY_OWNER[myPlayerId])
                    unit.SetAttackDamage((int) (unit.AttackDamage * multiplier));
        } },
        { "cost_reducer_buy", () =>
        {
            // store multiplier for future buys
            _MULTIPLIERS["cost_reducer_buy"] = 0.9f;
        } },
        { "cost_reducer_buy_2", () =>
        {
            // store multiplier for future buys
            _MULTIPLIERS["cost_reducer_buy"] = 0.8f;
        } },
    };

    public static void Apply(string code)
    {
        System.Action action;
        if (_ACTIONERS.TryGetValue(code, out action))
            action();
#if UNITY_EDITOR
        else
            Debug.LogWarning(
                $"No actioner defined for tech tree node '{code}' - can't apply!");
#endif
    }

    public static float GetMultiplier(string code)
    {
        float booster;
        if (_MULTIPLIERS.TryGetValue(code, out booster))
            return booster;
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                $"No booster defined for tech tree node '{code}' - using default of 1");
#endif
            return 1f;
        }
    }

}
