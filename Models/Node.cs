using System.ComponentModel.DataAnnotations;


namespace Coordinator.Models
{
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<NodeState> NodeState { get; set; }
    }
}
