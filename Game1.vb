Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Audio
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input
Imports Microsoft.Xna.Framework.Media

Public NotInheritable Class Game1
    Inherits Game

    Private ReadOnly _graphics As GraphicsDeviceManager
    Private _spriteBatch As SpriteBatch
    Private _player As Player, _coin As Coin, _gameFont As SpriteFont
    Private _peanutTitle As Texture2D
    Public Const VIEWPORT_WIDTH As Integer = 800
    Public Const VIEWPORT_HEIGHT As Integer = 600
    Public Shared ReadOnly Property CenterPos As Vector2
        Get
            Return New Vector2(VIEWPORT_WIDTH / 2, VIEWPORT_HEIGHT / 2)
        End Get
    End Property
    Private timeLeft As Single, isGameOver As Boolean, isGameStart As Boolean
    Private _mainTheme As Song, _coinSound As SoundEffect
    Private Property UsePeanut As Boolean

    Public Sub New(usePeanut As Boolean)
        _graphics = New GraphicsDeviceManager(Me) With {
            .PreferredBackBufferWidth = VIEWPORT_WIDTH,
            .PreferredBackBufferHeight = VIEWPORT_HEIGHT
        }
        _graphics.ApplyChanges()

        Content.RootDirectory = "Content"
        IsMouseVisible = False
        Me.UsePeanut = usePeanut
    End Sub

    Public Event ResetGame As EventHandler

    Public Function MapPlayerImage(direction As Direction) As Texture2D
        Select Case direction
            Case Direction.Up
                Return Content.Load(Of Texture2D)("player_up")
            Case Direction.Down
                Return Content.Load(Of Texture2D)("player_down")
            Case Direction.Left
                Return Content.Load(Of Texture2D)("player_left")
            Case Direction.Right
                Return Content.Load(Of Texture2D)("player_right")
            Case Else
                Throw New ArgumentException("Invalid direction for player movement.")
        End Select
    End Function

    Protected Overrides Sub Initialize()
        ' TODO: Add your initialization logic here
        _player = New Player(Content.Load(Of Texture2D)("player_right"), CenterPos)
        _coin = New Coin(Content.Load(Of Texture2D)(If(UsePeanut, "peanut", "coin_animation")))
        _gameFont = Content.Load(Of SpriteFont)("game_font")
        _mainTheme = Content.Load(Of Song)("main_theme")
        isGameStart = False

        ResetGameEvent = Sub(sender, e)
                             If isGameStart Then MediaPlayer.Play(_mainTheme)
                             _player.Score = 0
                             timeLeft = 60
                             _coin.SpawnRandomlyButAvoidPlayer(_player)
                             Task.Run(Sub() _player.Position = CenterPos)
                             isGameOver = False
                         End Sub
        RaiseEvent ResetGame(Nothing, EventArgs.Empty)

        MyBase.Initialize()
    End Sub

    Protected Overrides Sub LoadContent()
        _spriteBatch = New SpriteBatch(GraphicsDevice)

        ' TODO: use this.Content to load your game content here
        _peanutTitle = Content.Load(Of Texture2D)("peanut_title")
        _coinSound = Content.Load(Of SoundEffect)(If(UsePeanut, "peanut_sound", "coin_sound"))
        SoundEffect.MasterVolume = 0.8F
    End Sub

    Protected Overrides Sub Update(gameTime As GameTime)
        If GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed OrElse
            Keyboard.GetState().IsKeyDown(Keys.Escape) Then [Exit]()

        ' TODO: Add your update logic here
        Dim keyboardState = Keyboard.GetState()
        If isGameStart Then
            timeLeft -= CSng(gameTime.ElapsedGameTime.TotalSeconds)
            If timeLeft <= 0 Then
                isGameOver = True
                timeLeft = 0
            End If
        ElseIf keyboardState.IsKeyDown(Keys.Space) Then
            isGameStart = True
            MediaPlayer.Play(_mainTheme)
        End If
        
        Dim updatedPos = _player.Position
        If isGameStart AndAlso Not isGameOver Then
            If keyboardState.IsKeyDown(Keys.Left) Then
                updatedPos.X -= Player.MOVE_SPEED * CSng(gameTime.ElapsedGameTime.TotalSeconds)
                _player.Texture = MapPlayerImage(Direction.Left)
            End If
            If keyboardState.IsKeyDown(Keys.Right) Then
                updatedPos.X += Player.MOVE_SPEED * CSng(gameTime.ElapsedGameTime.TotalSeconds)
                _player.Texture = MapPlayerImage(Direction.Right)
            End If
            If keyboardState.IsKeyDown(Keys.Up) Then
                updatedPos.Y -= Player.MOVE_SPEED * CSng(gameTime.ElapsedGameTime.TotalSeconds)
                _player.Texture = MapPlayerImage(Direction.Up)
            End If
            If keyboardState.IsKeyDown(Keys.Down) Then
                updatedPos.Y += Player.MOVE_SPEED * CSng(gameTime.ElapsedGameTime.TotalSeconds)
                _player.Texture = MapPlayerImage(Direction.Down)
            End If
        ElseIf isGameOver AndAlso keyboardState.IsKeyDown(Keys.R) Then
            RaiseEvent ResetGame(Nothing, EventArgs.Empty)
        End If
        _player.Position = updatedPos
        _player.WrapPosition()
        _player.CollectCoin(_coin, _coinSound)

        MyBase.Update(gameTime)
    End Sub

    Protected Overrides Sub Draw(gameTime As GameTime)
        GraphicsDevice.Clear(Color.Black)
        Const HUD_HEIGHT = 75
        Const TITLE_DRAWING_SCALE = 2

        ' TODO: Add your drawing code here
        _spriteBatch.Begin(samplerState:=SamplerState.PointClamp)
        _player.Draw(_spriteBatch)
        If Not UsePeanut Then
            _coin.Frame = CInt(gameTime.TotalGameTime.TotalSeconds * 10) Mod 3
        Else
            Dim belowTitleY = VIEWPORT_HEIGHT - _peanutTitle.Height * TITLE_DRAWING_SCALE
            _spriteBatch.Draw(
                _peanutTitle,
                New Vector2(VIEWPORT_WIDTH - _peanutTitle.Width * TITLE_DRAWING_SCALE,
                            If(_coin.Position.Y > HUD_HEIGHT, 0, belowTitleY)),
                New Rectangle(0, 0, _peanutTitle.Width, _peanutTitle.Height),
                Color.White,
                0,
                Vector2.Zero,
                TITLE_DRAWING_SCALE,
                SpriteEffects.None,
                0
            )
        End If
        _coin.Draw(_spriteBatch)

        _spriteBatch.DrawString(
            _gameFont, $"Score: {_player.Score,2} | Highest: {_player.Highest,2}

Time Left: {timeLeft,2:F0}",
            If(_coin.Position.Y > HUD_HEIGHT, New Vector2(10, 10),
                New Vector2(10, VIEWPORT_HEIGHT - HUD_HEIGHT)),
            Color.White
        )
        If Not isGameStart Then
            _spriteBatch.DrawString(
                _gameFont,
                "Press 'SPACE' to begin the game.",
                New Vector2(75, VIEWPORT_HEIGHT / 2 - 50),
                Color.White
            )
        ElseIf isGameOver Then
            _spriteBatch.DrawString(
                _gameFont,
                "GAME OVER! Press 'R' to restart.",
                New Vector2(75, VIEWPORT_HEIGHT / 2),
                Color.White
            )
        End If
        _spriteBatch.End()

        MyBase.Draw(gameTime)
    End Sub

    Friend Shared Sub Main()
        System.Windows.Forms.Application.EnableVisualStyles()
        Dim usePeanut As Boolean = (MsgBox("Use peanuts as collectibles during gameplay?
If not, simply click 'Cancel' to continue.",
            MsgBoxStyle.OkCancel, "Welcome to Coin Collector!") = MsgBoxResult.Ok)
        Call New Game1(usePeanut).Run()
    End Sub
End Class
