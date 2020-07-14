package main

import (
	"io/ioutil"
//	"net"
	"net/http"
	"net/url"
	"fmt"
	"bytes"
	"bufio"
	"log"
    "github.com/perlin-network/noise"
	"github.com/perlin-network/noise/kademlia"
	"go.uber.org/zap"
	"os"
	"os/signal"
	"strings"
	"context"
	"time"
    "strconv"
)

// printedLength is the total prefix length of a public key associated to a chat users ID.
const printedLength = 8

func help(node *noise.Node) {
	fmt.Printf("Your ID is %s(%s)\n",
		node.ID().Address,
		node.ID().ID.String()[:printedLength],
	)
}


// peers prints out all peers we are already aware of.
func peers(overlay *kademlia.Protocol) {
	ids := overlay.Table().Peers()

	var str []string
	for _, id := range ids {
		str = append(str, fmt.Sprintf("%s(%s)", id.Address, id.ID.String()[:printedLength]))
	}

	fmt.Printf("You know %d peer(s): [%v]\n", len(ids), strings.Join(str, ", "))
}

// discover uses Kademlia to discover new peers from nodes we already are aware of.
func discover(overlay *kademlia.Protocol) {
	ids := overlay.Discover()

	var str []string
	for _, id := range ids {
		str = append(str, fmt.Sprintf("%s(%s)", id.Address, id.ID.String()[:printedLength]))
	}

	if len(ids) > 0 {
		fmt.Printf("Discovered %d peer(s): [%v]\n", len(ids), strings.Join(str, ", "))
	} else {
		fmt.Printf("Did not discover any peers.\n")
	}
}

// bootstrap pings and dials an array of network addresses which we may interact with and  discover peers from.
func bootstrap(node *noise.Node, addresses ...string) {
	for _, addr := range addresses {
		ctx, cancel := context.WithTimeout(context.Background(), 3*time.Second)
		_, err := node.Ping(ctx, addr)
		cancel()

		if err != nil {
			fmt.Printf("Failed to ping bootstrap node (%s). Skipping... [error: %s]\n", addr, err)
			continue
		}
	}
}

//
// func listenForOutgoingTraffic(hostNode *noise.Node, hostPort int, destPort int) {
// 	// Listen for incoming connections.
//     l, err := net.Listen("tcp", "localhost:" + string(destPort))
//     if err != nil {
//         fmt.Println("Error listening:", err.Error())
//         os.Exit(1)
//     }
	
// 	// Close the listener when the application closes.
// 	defer l.Close()
	
//     for {
//         // Listen for an incoming connection.
//         conn, err := l.Accept()
//         if err != nil {
//             fmt.Println("Error accepting: ", err.Error())
//             os.Exit(1)
//         }
//         // Handle connections in a new goroutine.
//         go handleOutgoingRequest(hostNode, hostPort, conn)
//     }
// }

// handleOutgoingRequest
// func handleOutgoingRequest(hostNode *noise.Node, hostPort int, conn net.Conn) {
// 	defer conn.Close()
// 	reqData, err := ioutil.ReadAll(conn)
// 	if err != nil {
// 		log.Fatal(err)
// 	}
// 	req, err := http.ReadRequest(bufio.NewReaderSize(bytes.NewReader(reqData), 1024))
// 	if err != nil {
// 		log.Fatal(err)
// 	}

// 	reqDestination := req.Header["Peer-Destination"][0]
// 	reqDestinationType := req.Header["Peer-Destination-Type"][0]

// 	switch (reqDestinationType) {
// 	// case "id":
// 	// 	reqNode, err := hostNode.Ping(context.TODO(), reqDestination)
// 	// 	break;
// 	case "address":
// 		err = hostNode.Send(context.TODO(), reqDestination, reqData)
// 		break;
// 	default:
// 		log.Fatal("Invalid destination type: " + reqDestinationType)
// 		break;
// 	}
// 	if err != nil {
// 		log.Fatal(err)
// 	}

// 	// responseData, err := ioutil.ReadAll(conn)
// 	// if err != nil {
// 	// 	log.Fatal(err)
// 	// }
// }

// This example demonstrates how to send/handle RPC requests across peers, how to listen for incoming
// peers, how to check if a message received is a request or not, how to reply to a RPC request, and
// how to cleanup node instances after you are done using them.
func main() {
	listenPort, err := strconv.ParseInt(os.Args[1], 10, 64) // 9000
	if err != nil {
		panic(err)
	}

	hostPort, err := strconv.ParseInt(os.Args[2], 10, 64) // 12524
	if err != nil {
		panic(err)
	}

	logger, err := zap.NewDevelopment(zap.AddStacktrace(zap.PanicLevel))
	if err != nil {
		panic(err)
	}
	defer logger.Sync()

	node, err := noise.NewNode(noise.WithNodeLogger(logger), noise.WithNodeBindPort(uint16(listenPort)))
	if err != nil {
		panic(err)
	}
	defer node.Close()

	// Forward HTTP requests to local host
	node.Handle(func(ctx noise.HandlerContext) error {
		req, err := http.ReadRequest(bufio.NewReaderSize(bytes.NewReader(ctx.Data()), 1024))
		if err != nil {
			log.Fatal(err)
		}
		reqUrl := "http://localhost:" + string(hostPort) + "/Api/Peer/Msg?url=" + url.QueryEscape(req.URL.String())
		switch (req.Method) {
		case "Post":
			resp, err := http.Post(reqUrl, req.Header["Content-Type"][0], req.Body)
			if err != nil {
				log.Fatal(err)
			}
			data, err := ioutil.ReadAll(resp.Body)
			resp.Body.Close()
			if err != nil {
				log.Fatal(err)
			}
			ctx.Send(data)
			break;
		}
		return nil
	})

	// Instantiate Kademlia.
	events := kademlia.Events{
		OnPeerAdmitted: func(id noise.ID) {
			fmt.Printf("Learned about a new peer %s(%s).\n", id.Address, id.ID.String()[:printedLength])
		},
		OnPeerEvicted: func(id noise.ID) {
			fmt.Printf("Forgotten a peer %s(%s).\n", id.Address, id.ID.String()[:printedLength])
		},
	}

	overlay := kademlia.New(kademlia.WithProtocolEvents(events))

	node.Bind(overlay.Protocol())

	if err := node.Listen(); err != nil {
		panic(err)
	}

	help(node)

	node.Ping(context.TODO(), "localhost:8000")

	c := make(chan os.Signal, 1)
	signal.Notify(c, os.Interrupt)
	<-c
}