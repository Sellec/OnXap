﻿namespace OnXap.Messaging.DB
{
    enum MessageStateType : byte
    {
        NotProcessed = 0,
        Complete = 1,
        Error = 2,
        Repeat = 4,
        IntermediateAdded = 5,
    }
}
