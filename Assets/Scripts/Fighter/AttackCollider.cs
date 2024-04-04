using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        // check that the other is not us
        string output = "Attack OnTriggerEnter\n";
        output += other + "\n";
        output += transform + "\n";
        output += other.transform + "\n";

        if (other.transform != transform && other.TryGetComponent<Fighter>(out Fighter fighter)) {
            output += "Attack OnTriggerEnter: Fighter\n";
            output += fighter + "\n";
            output += fighter.transform + "\n";

            // fighter.TakeDamage(10f);
        }
        Debug.Log(output);
    }
}
