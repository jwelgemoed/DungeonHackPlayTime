using System;

namespace FunAndGamesWithSharpDX.Engine
{
    public class CollissionEventArgs : EventArgs
    {
        public object CollidedObject { get; set; }
    }
}