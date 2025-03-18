namespace Lapo.Mud.Options;

public class AluOptions
{
    public string TableColumn { get; set; } = $"{Guid.NewGuid()}";
    public string OperationColumn { get; set; } = $"{Guid.NewGuid()}";
    public string PrimaryKeyColumn { get; set; } = $"{Guid.NewGuid()}";
}