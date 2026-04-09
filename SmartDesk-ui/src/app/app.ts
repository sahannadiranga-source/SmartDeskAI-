import { Component, signal, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ChatService, ChatResponse } from './chat.service';

interface Message {
  role: 'user' | 'bot';
  text: string;
  sentiment?: number;
  escalation?: boolean;
}

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements AfterViewChecked {
  @ViewChild('messageList') messageList!: ElementRef;

  messages = signal<Message[]>([]);
  userInput = '';
  sessionId = '';
  loading = false;

  constructor(private chatService: ChatService) {}

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  send() {
    const text = this.userInput.trim();
    if (!text || this.loading) return;

    this.messages.update((msgs) => [...msgs, { role: 'user', text }]);
    this.userInput = '';
    this.loading = true;

    this.chatService.sendMessage(text, this.sessionId).subscribe({
      next: (res: ChatResponse) => {
        this.sessionId = res.sessionId;
        this.messages.update((msgs) => [
          ...msgs,
          {
            role: 'bot',
            text: res.answer,
            sentiment: res.sentiment,
            escalation: res.priority_Escalation,
          },
        ]);
        this.loading = false;
      },
      error: () => {
        this.messages.update((msgs) => [
          ...msgs,
          { role: 'bot', text: 'Something went wrong. Please try again.' },
        ]);
        this.loading = false;
      },
    });
  }

  reset() {
    if (this.sessionId) {
      this.chatService.resetSession(this.sessionId).subscribe();
    }
    this.messages.set([]);
    this.sessionId = '';
  }

  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  sentimentLabel(score: number): string {
    if (score >= 0.6) return 'Very Positive';
    if (score >= 0.2) return 'Positive';
    if (score >= -0.2) return 'Neutral';
    if (score >= -0.6) return 'Negative';
    return 'Very Negative';
  }

  sentimentClass(score: number): string {
    if (score >= 0.2) return 'positive';
    if (score >= -0.2) return 'neutral';
    return 'negative';
  }

  private scrollToBottom() {
    try {
      this.messageList.nativeElement.scrollTop = this.messageList.nativeElement.scrollHeight;
    } catch {}
  }
}
