using ShootGameServer.SharedData;

namespace ShootGameServer
{
    //玩家发过来的指令信息
    class PlayerMessage
    {
        static public void Execute(ClientMessageCarrior msg, GameServer server)
        {
            try
            {
                string id = msg.id;
                int code = msg.message.stateCode;

                switch (code)
                {
                    case MsgCode.CLIENT_MOVE:
                        ClientMoveMessage m = msg.message as ClientMoveMessage;
                        server.MoveAndFire(id, m.dx, m.dy);
                        break;
                    case MsgCode.CLIENT_QUIT:
                        server.Quit(id);
                        break;
                    //case Fire:
                    //    server.Fire(id);
                    //    break;
                    default:
                        break;
                }
            }
            catch
            {

            }
        }
    }
}