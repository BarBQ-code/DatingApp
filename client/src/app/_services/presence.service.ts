import { Injectable } from '@angular/core';
import {environment} from "../../environments/environment";
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {ToastrService} from "ngx-toastr";
import {User} from "../_models/user";
import {BehaviorSubject} from "rxjs";
import {take} from "rxjs/operators";
import {Router} from "@angular/router";

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnetion: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();


  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnetion = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory : () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnetion
      .start()
      .catch(error => console.log(error));

    this.hubConnetion.on('UserIsOnline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        this.onlineUsersSource.next([...usernames, username]);
      });
    });

    this.hubConnetion.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        this.onlineUsersSource.next([...usernames.filter(x => x !== username)]);
      })
    });

    this.hubConnetion.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    });

    this.hubConnetion.on('NewMessageReceived', ({username, knowAs}) => {
      this.toastr.info(knowAs + ' has sent you a new message!')
        .onTap
        .pipe(take(1))
        .subscribe(() => {
          this.router.navigateByUrl('/members/' + username + '?tab=3');
        });
    });
  }

  stopHubConnection() {
    this.hubConnetion
      .stop()
      .catch(error => console.log(error));
  }

}
