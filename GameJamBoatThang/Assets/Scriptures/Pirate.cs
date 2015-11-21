using UnityEngine;
using System.Collections;
using Rewired;

[RequireComponent(typeof(Rigidbody))]
public class Pirate : MonoBehaviour
{
    public int playerIndex;
    Player player;

    Rigidbody myBody;

    float xIn;
    float yIn;

    public float moveSpeed;
    Vector3 velo = Vector3.zero;

	// Use this for initialization
	void Start ()
    {
        player = ReInput.players.GetPlayer(playerIndex);

        myBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update ()
    {
        xIn = player.GetAxis("MoveHorizontal");
        yIn = player.GetAxis("MoveVertical");

        velo.x = xIn * moveSpeed;
        velo.z = yIn * moveSpeed;

        myBody.velocity = velo;
    }
}
