using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] GameObject notification;
    [SerializeField] Vector3 spawnOffset;
    [SerializeField] List<Notification> notificationQueue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            GameObject notif = Instantiate(notification, this.transform);
            notif.transform.localPosition = spawnOffset;
        }
    }
}
