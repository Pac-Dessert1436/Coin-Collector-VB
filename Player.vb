Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Audio
Imports Microsoft.Xna.Framework.Graphics

Public Enum Direction
    Up = 1
    Down = 2
    Left = 4
    Right = 8
End Enum

Public Class Player
    Public Sub New(texture As Texture2D, position As Vector2)
        Me.Texture = texture
        Me.Position = position
    End Sub

    Public Property Texture As Texture2D
    Public Property Position As Vector2

    Public ReadOnly Property Bounds As Rectangle
        Get
            Return New Rectangle(
                Position.X, Position.Y, Texture.Width * DRAWING_SCALE,
                Texture.Height * DRAWING_SCALE
            )
        End Get
    End Property

    Public Const DRAWING_SCALE As Integer = 2
    Public Const MOVE_SPEED As Integer = 350
    Public Property Score As Integer = 0
    Public Property Highest As Integer = 0

    Public Sub CollectCoin(coin As Coin, sfx As SoundEffect)
        If Bounds.Intersects(coin.Bounds) Then
            sfx.Play()
            Score += 1
            coin.SpawnRandomlyButAvoidPlayer(Me)
        End If
        If Score > Highest Then Highest = Score
    End Sub

    Public Sub WrapPosition()
        Dim updatedPos = Position
        If updatedPos.X < 0 Then
            updatedPos.X = Game1.VIEWPORT_WIDTH - Texture.Width * DRAWING_SCALE
        ElseIf updatedPos.X > Game1.VIEWPORT_WIDTH - Texture.Width * DRAWING_SCALE Then
            updatedPos.X = 0
        End If
        If updatedPos.Y < 0 Then
            updatedPos.Y = Game1.VIEWPORT_HEIGHT - Texture.Height * DRAWING_SCALE
        ElseIf updatedPos.Y > Game1.VIEWPORT_HEIGHT - Texture.Height * DRAWING_SCALE Then
            updatedPos.Y = 0
        End If
        Position = updatedPos
    End Sub

    Public Sub Draw(spriteBatch As SpriteBatch)
        spriteBatch.Draw(
            Texture,
            Position,
            New Rectangle(0, 0, Texture.Width, Texture.Height),
            Color.White,
            0,
            Vector2.Zero,
            DRAWING_SCALE,
            SpriteEffects.None,
            0
        )
    End Sub
End Class
