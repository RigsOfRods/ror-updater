
namespace ror_updater
{
    public interface ISwitchable
    {
        void UtilizeState(object state);
        void recvData(string[] str, int[] num);
    }
}
