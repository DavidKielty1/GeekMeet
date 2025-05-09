import { Component, computed, inject, input, Input } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { RouterLink } from '@angular/router';
import { LikesService } from 'src/app/_services/likes.service';
import { PresenceService } from 'src/app/_services/presence.service';
@Component({
    selector: 'app-member-card',
    templateUrl: './member-card.component.html',
    styleUrls: ['./member-card.component.css'],
    standalone: true,
    imports: [RouterLink],
})
export class MemberCardComponent {
  private likeService = inject(LikesService)
  private presenceService = inject(PresenceService)
  member = input.required<Member>()
  hasLiked = computed(() => this.likeService.likeIds().includes(this.member().id))
  isOnline = computed(() => this.presenceService.onlineUsers().includes(this.member().userName))

  toggleLike() {
    this.likeService.toggleLike(this.member().id).subscribe({
      next: () => {
        if (this.hasLiked()) {
          this.likeService.likeIds.update(ids => ids.filter(id => id !== this.member().id))
        } else {
          this.likeService.likeIds.update(ids => [...ids, this.member().id])
        }
      }
    })
  }
}
