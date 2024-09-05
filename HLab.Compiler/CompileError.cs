namespace HLab.Compiler;

public class CompileError
{
    public string Message { get; set; } = "";
    public int Line { get; set; }
    public int Pos { get; set; }
    public int Length { get; set; }
}