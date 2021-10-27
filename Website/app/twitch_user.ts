import axios from "axios";
import { query } from "express";
import {Db, MongoClient, ObjectId} from "mongodb";

const uri = "dbplaceholder";
const client = new MongoClient(uri, { useNewUrlParser: true, useUnifiedTopology: true });

export class TwitchUser {
    access_token: string;
    expires_in: number;
    refresh_token: string;
    scope: string[];
    username: string;

    constructor(access_token: string, expires_in: number, refresh_token: string, scope: string[]){
        this.access_token = access_token;
        this.expires_in = expires_in;
        this.refresh_token = refresh_token;
        this.scope = scope;
        this.username = this.grabUserName(access_token);
    }
    private grabUserName = (access_token: string) =>{
        let name: string = "NOT SET";


        //Header Configuration
        let headerCfg = {
            headers: {
                Authorization: `Bearer ${access_token}`,
                'Client-Id': `placeholder`,
            }
        }

        //Gets the relevant user infomartion from twitch, and maps username to name
        axios.get("https://api.twitch.tv/helix/users", headerCfg)
        .then(res => {
            name = res.data.data[0].login;

            this.username = name;
        }).catch(error => {
            console.log(error);
        });

        return name;
    }

    SubmitToDb = (uName: string)=>{
        client.connect(err => {
            if (err) throw err;
            const collection = client.db("txtcommands").collection("pgoUserInfo");
            const queryFilter = { username: uName};

            if(collection.findOne()){
                
            }

            let user = {
                access_token: this.access_token,
                expires_in: this.expires_in,
                refresh_toke: this.refresh_token,
                scope: this.scope,
                username: this.username,
            }

            collection.insertOne(user, (err, res)=> {
                if(err) throw err;
                console.log("document inserted");
                client.close();
            } );

        });
        
    }


}