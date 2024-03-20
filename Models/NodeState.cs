using Coordinator.enums;

namespace Coordinator.Models
{
    public class NodeState
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public ReadyType IsReady { get; set; }

        public TransactionState TransactionState { get; set; }
        public Node Node { get; set; }

    }
}
