using System;

namespace FunAndGamesWithSlimDX.Engine
{
    public class CollissionEventArgs : EventArgs
    {
        public object CollidedObject { get; set; }
    }
}