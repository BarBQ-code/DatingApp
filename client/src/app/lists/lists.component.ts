import { Component, OnInit } from '@angular/core';
import {Member} from "../_models/member";
import {MembersService} from "../_services/members.service";
import {likesParams} from "../_models/LikesParams";
import {Pagination} from "../_models/pagination";

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Partial<Member[]>;
  likesParams = new likesParams();
  pagination: Pagination;

  constructor(private memberService: MembersService) { }

  ngOnInit(): void {
    this.loadLikes();
  }

  loadLikes() {
    this.memberService.getLikes(this.likesParams).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination;
    });
  }

  pageChanged(event: any) {
    this.likesParams.pageNumber = event.page;
    this.loadLikes();
  }

}
