// For Doors, Vents, Windows, etc.
public interface IOpenable{
    bool IsOpen {get; set;}

    int Keys { get; set; }

    void AddKey();
    void RemoveKey();
    void Toggle();
    void Open();
    void Close();
    void Setup();
}