import express, { request } from 'express';
import path from 'path';
import * as WebSocket from 'ws';
import axios, { AxiosResponse } from 'axios';
import { TwitchUser } from './twitch_user';

let PORT: number = 3000; 
let client_id: string = "placeholder";
let client_secret: string = "placeholder";
let redirectUri: string = "http://localhost:3000";

const app: express.Application = express();
const wsServer = new WebSocket.Server({noServer: true});

wsServer.on('connection', socket => {
    socket.on('message', message => console.log(message));
});

//Setting static folder
//app.use(express.static(path.join(__dirname, 'public')))

app.get('/', async (req, res) => {
    let authCode: any = req.query.code;

    res.sendFile((path.join(__dirname, "/public/index.html")));

    if(authCode !== undefined){

        console.log(authCode);
        axios.post(
            `https://id.twitch.tv/oauth2/token?client_id=${ client_id }&client_secret=${ client_secret }&code=${ authCode }&grant_type=authorization_code&redirect_uri=${ redirectUri }`
        ).then(res => {
            console.log(`status code: ${ res.status }`);
            let newUser = new TwitchUser(res.data.access_token, res.data.expires_in, res.data.refresh_token, res.data.scope);
            setTimeout(() => {
                newUser.SubmitToDb();
            }, 1000);
        }).catch(error => {
            console.log(error);
        });
        
    }

});

const server = app.listen(PORT, function () {
    console.log('app listening on port '+ PORT +'!');
});

server.on('upgrade', (request, socket, head) => {
    wsServer.handleUpgrade(request, socket, head, socket =>{
        wsServer.emit('connection', socket, request);
    })
})