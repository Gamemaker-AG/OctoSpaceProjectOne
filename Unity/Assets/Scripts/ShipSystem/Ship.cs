using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour
{

    //Benötigt für das Laden der ShipConfig
    public TextAsset ShipConfigs;
    private List<Dictionary<string, string>> shipDictionaryList = new List<Dictionary<string, string>>();
    private Dictionary<int, string> shipDictionary; 


    public string ShipID { get; set; }
    public int ShipType = -1;
    public Engineer MyEngineer; // { get; set; }
    public string name;

	// Use this for initialization
	void Start ()
	{
	    MyEngineer = gameObject.AddComponent<Engineer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void GetShipTypeFromXML()
    {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(ShipConfigs.text);
        XmlNodeList shipsNodeList = xmlDocument.GetElementsByTagName("Ship");

        foreach (XmlNode shipList in shipsNodeList)
        {
            XmlNodeList shipInfoList = shipList.ChildNodes;
            // TODO XMLParser fertig stellen
            // um die Information aus der ShipInfo.xml zu lesen
            // Wenn dies geschehen ist, können wir dem schiff die Properties zuordnen und Der Engineer kann sich diese ebenfalls auslesen
            // Momentan wäre es einfach die Gesamte Konfiguration Statisch zu machen, ein Upgradesystem kann im späteren Verlauf des Projektes implementiert werden
        }

    }
}
