using Godot;
using System;
using System.Security.Cryptography.X509Certificates;
using ParadisisNostalga;
using System.Runtime.CompilerServices;

public partial class NEwMain : Node2D
{
    private string journalCampaign;
    private bool quitConfig;

    public bool isDevMode = false;

    public override void _Ready()
    {
        Button journalButton = new Button();
        journalButton.Text = "Journal";
        journalButton.Connect("pressed", new Callable(this, nameof(OnJournalButtonPressed)));
        AddChild(journalButton);

        Button campaignButton = new Button();
        campaignButton.Text = "Campaign";
        campaignButton.Connect("pressed", new Callable(this, nameof(OnCampaignButtonPressed)));
        AddChild(campaignButton);

        Button quitButton = new Button();
        quitButton.Text = "Quit";
        quitButton.Connect("pressed", new Callable(this, nameof(OnQuitButtonPressed)));
        AddChild(quitButton);

        Button startButton = new Button();
        startButton.Text = "Start";
        startButton.Connect("pressed", new Callable(this, nameof(OnStartButtonPressed)));
        AddChild(startButton);

        Button devTestButton = new Button();
        devTestButton.Text = "Dev Test";
        devTestButton.Connect("pressed", new Callable(this, nameof(OnDevTestButtonPressed)));
        AddChild(devTestButton);
    }

    private void OnJournalButtonPressed()
    {
        GD.Print("Journal button pressed");
        GetTree().ChangeSceneToFile("res://JournalScene.tscn");
    }

    private void OnCampaignButtonPressed()
    {
        GD.Print("Campaign button pressed");
        GetTree().ChangeSceneToFile("res://CampaignSelectionScene.tscn");
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnStartButtonPressed()
    {
        GD.Print("Start button pressed");
        GetTree().ChangeSceneToFile("res://LastSavedGameScene.tscn");
    }

    private void OnDevTestButtonPressed()
    {
        GD.Print("Dev Test button pressed");

        bool isDevMode = true;
        SaveManager.DevTheatreSave(new TheatreSaveData
        {
            TheatreName = "DevTestTheatre",
        });
        GetTree().ChangeSceneToFile("res://world.tscn");

        
    }
}
