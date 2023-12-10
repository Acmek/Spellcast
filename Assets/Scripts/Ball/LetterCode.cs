using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Photon.Pun;

public class LetterCode : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        if(!PhotonNetwork.InRoom || (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)) {
            float rnd = Random.Range(0.0f, 100.0f);

            if(rnd >= 0 && rnd <= 11.1607f)
                GetComponent<TMP_Text>().text = "E";
            else if(rnd > 11.1607f && rnd <= 19.6573f)
                GetComponent<TMP_Text>().text = "A";
            else if(rnd > 19.6573f && rnd <= 27.2382f)
                GetComponent<TMP_Text>().text = "R";
            else if(rnd > 27.2382f && rnd <= 34.783f)
                GetComponent<TMP_Text>().text = "I";
            else if(rnd > 34.783f && rnd <= 41.9465f)
                GetComponent<TMP_Text>().text = "O";
            else if(rnd > 41.9465f && rnd <= 48.8974f)
                GetComponent<TMP_Text>().text = "T";
            else if(rnd > 48.8974f && rnd <= 55.5518f)
                GetComponent<TMP_Text>().text = "N";
            else if(rnd > 55.5518f && rnd <= 61.2869f)
                GetComponent<TMP_Text>().text = "S";
            else if(rnd > 61.2869f && rnd <= 66.7762f)
                GetComponent<TMP_Text>().text = "L";
            else if(rnd > 66.7762f && rnd <= 71.315f)
                GetComponent<TMP_Text>().text = "C";
            else if(rnd > 71.315f && rnd <= 74.9458f)
                GetComponent<TMP_Text>().text = "U";
            else if(rnd > 74.9498f && rnd <= 78.3302f)
                GetComponent<TMP_Text>().text = "D";
            else if(rnd > 78.3302f && rnd <= 81.4973f)
                GetComponent<TMP_Text>().text = "P";
            else if(rnd > 81.4973f && rnd <= 84.5102f) {
                GetComponent<TMP_Text>().text = "M";
                GetComponent<TMP_Text>().fontStyle = FontStyles.Bold | FontStyles.Underline;
            }
            else if(rnd > 84.5102f && rnd <= 87.5136f)
                GetComponent<TMP_Text>().text = "H";
            else if(rnd > 87.5136f && rnd <= 89.9841f)
                GetComponent<TMP_Text>().text = "G";
            else if(rnd > 89.9841f && rnd <= 92.0561f)
                GetComponent<TMP_Text>().text = "B";
            else if(rnd > 92.0561f && rnd <= 93.8682f)
                GetComponent<TMP_Text>().text = "F";
            else if(rnd > 93.8682f && rnd <= 95.6461f)
                GetComponent<TMP_Text>().text = "Y";
            else if(rnd > 95.6461f && rnd <= 96.936f) {
                GetComponent<TMP_Text>().text = "W";
                GetComponent<TMP_Text>().fontStyle = FontStyles.Bold | FontStyles.Underline;
            }
            else if(rnd > 96.936f && rnd <= 98.0376f)
                GetComponent<TMP_Text>().text = "K";
            else if(rnd > 98.0376f && rnd <= 99.045f)
                GetComponent<TMP_Text>().text = "V";
            else if(rnd > 99.045f && rnd <= 99.3352f)
                GetComponent<TMP_Text>().text = "X";
            else if(rnd > 99.3352f && rnd <= 99.6074f)
                GetComponent<TMP_Text>().text = "Z";
            else if(rnd > 99.6074f && rnd <= 99.8039f)
                GetComponent<TMP_Text>().text = "J";
            else
                GetComponent<TMP_Text>().text = "Q";

            if(PhotonNetwork.IsMasterClient && !transform.parent.parent.GetComponent<BallCode>().IsPowerup()) {
                transform.parent.parent.GetComponent<BallCode>().ChangeBall(GetComponent<TMP_Text>().text, transform.parent.parent.GetChild(2).GetComponent<SpriteRenderer>().color.r, transform.parent.parent.GetChild(2).GetComponent<SpriteRenderer>().color.g, transform.parent.parent.GetChild(2).GetComponent<SpriteRenderer>().color.b);
            }
        }
    }
}
