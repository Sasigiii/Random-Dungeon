public enum AimDirection
{
    Up,
    UpRight,
    Right,
    Down,
    Left,
    UpLeft
}

public enum GameState
{
    GameStarted,
    PlayingLevel,
    EngagingBoss,
    EngagingEnemies,
    LevelCompleted,
    GameWon,
    GameLost,
    GamePaused,
    DungeonOverviewMap,
    RestartGame,
    BossStage,
}

public enum Orientation
{
    North,
    East,
    South,
    West,
    None
}