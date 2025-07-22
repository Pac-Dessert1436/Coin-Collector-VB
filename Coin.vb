Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class Coin
    Public Property Texture As Texture2D
    Public Property Position As Vector2
    Private ReadOnly Property OriginScale As Integer

    Public Sub New(texture As Texture)
        Me.Texture = texture
        OriginScale = If(texture.Name = "peanut", 15, 10)
    End Sub

    Public Const DRAWING_SCALE As Integer = 2

    Public ReadOnly Property Bounds As Rectangle
        Get
            Return New Rectangle(
                Position.X, Position.Y, 
                OriginScale * DRAWING_SCALE, OriginScale * DRAWING_SCALE
            )
        End Get
    End Property

    Private ReadOnly random As New Random

    Public Sub SpawnRandomlyButAvoidPlayer(player As Player)
        Do
            Position = New Vector2(
                random.Next(0, Game1.VIEWPORT_WIDTH - Texture.Width * DRAWING_SCALE),
                random.Next(0, Game1.VIEWPORT_HEIGHT - Texture.Height * DRAWING_SCALE)
            )
        Loop While Bounds.Intersects(player.Bounds)
    End Sub

    Public Property Frame As Integer = 0

    Public Sub Draw(spriteBatch As SpriteBatch)
        spriteBatch.Draw(
            Texture, Position, 
            New Rectangle(Frame * OriginScale, 0, OriginScale, OriginScale), 
            Color.White,
            0, 
            Vector2.Zero, 
            DRAWING_SCALE, 
            SpriteEffects.None, 
            0
        )
    End Sub
End Class
