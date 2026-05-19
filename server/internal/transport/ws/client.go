package ws

import (
	"encoding/json"
	"log"

	"github.com/Lybertyxz/CatRoyale/server/pkg/protocol"
	"github.com/gofiber/contrib/websocket"
)

type Client struct {
	ID       string
	UserID   string
	Username string
	Send     chan []byte
	Hub      *Hub
	conn     *websocket.Conn
}

func NewClient(id, userID, username string, conn *websocket.Conn, hub *Hub) *Client {
	return &Client{
		ID:       id,
		UserID:   userID,
		Username: username,
		Send:     make(chan []byte, 256),
		Hub:      hub,
		conn:     conn,
	}
}

// ReadPump lit les messages entrants du client
func (c *Client) ReadPump() {
	defer func() {
		c.Hub.Unregister <- c
	}()

	for {
		_, msg, err := c.conn.ReadMessage()
		if err != nil {
			break
		}

		var envelope protocol.Envelope
		if err := json.Unmarshal(msg, &envelope); err != nil {
			log.Printf("invalid message from %s: %v", c.UserID, err)
			continue
		}

		c.Hub.Incoming <- &IncomingMessage{
			Client:   c,
			Envelope: envelope,
		}
	}
}

// WritePump envoie les messages sortants au client
func (c *Client) WritePump() {
	defer c.conn.Close()

	for msg := range c.Send {
		if err := c.conn.WriteMessage(websocket.TextMessage, msg); err != nil {
			break
		}
	}
}

// SendEnvelope envoie un message structuré au client
func (c *Client) SendEnvelope(msgType protocol.MessageType, payload interface{}) error {
	data, err := json.Marshal(payload)
	if err != nil {
		return err
	}

	envelope, err := json.Marshal(protocol.Envelope{
		Type:    msgType,
		Payload: data,
	})
	if err != nil {
		return err
	}

	c.Send <- envelope
	return nil
}
