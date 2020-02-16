using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//requests made by enemies to the manager
class EnemyRequest
{
    public EnemyRequestType requestType;
    public EnemyRequestResult requestResult = EnemyRequestResult.UNPROCESSED;
    public object requestObj;
    public bool complete = false;

    public EnemyRequest(EnemyRequestType requestType, object requestObj)
    {
        this.requestType = requestType;
        this.requestObj = requestObj;
    }
}

enum EnemyRequestType {LINE_OF_SIGHT, TRANSPORT}
enum EnemyRequestResult {UNPROCESSED, WAITING, SUCCESS, FAILURE}
