using System;
using System.Collections.Generic;

public class GatewayPayload : Dictionary<string, object>
{    
    public GatewayOpCode OpCode //OpCode for the payload
    {
        get
        {
            object op = this["op"];
            if (op == null)
            {
                throw new Exception("OpCode not present");
            }
            
            GatewayOpCode opCode;
            if (!GatewayOpCode.TryParse(op.ToString(), out opCode))
            {
                throw new Exception($"Invalid OpCode: {op}");
            }

            return opCode;
        }
        set { this["op"] = value; }
    }

    public int? SequenceNumber //Sequence number
    {
        get
        {
            object s = this["s"];
            if (s == null)
            {
                return null;
            }
            int sequenceNumber;
            if (!int.TryParse(this["s"].ToString(), out sequenceNumber))
            {
                throw new Exception($"Invalid sequence number: {s}");
            }

            return sequenceNumber;
        }
        set { this["s"] = value; }
    }

    public string EventName //Event name for the payload
    {
        get
        {
            return this["t"]?.ToString();
        }
        set { this["t"] = value; }
    }

    public object Data //Event data;
    {
       
        get { return this["d"];
            
        }
        set { this["d"] = value; }
    }

}
