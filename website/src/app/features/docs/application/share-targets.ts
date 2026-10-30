export interface ShareTarget {
  readonly label: string;
  readonly icon: string;
  href(url: string, title: string): string;
}

export interface AiTarget {
  readonly label: string;
  readonly icon: string;
  href(prompt: string): string;
}

const encode = encodeURIComponent;

export const SHARE_TARGETS: readonly ShareTarget[] = [
  { label: 'LinkedIn', icon: 'bi-linkedin', href: url => `https://www.linkedin.com/sharing/share-offsite/?url=${encode(url)}` },
  { label: 'X', icon: 'bi-twitter-x', href: (url, title) => `https://x.com/intent/post?url=${encode(url)}&text=${encode(title)}` },
  { label: 'Bluesky', icon: 'bi-bluesky', href: (url, title) => `https://bsky.app/intent/compose?text=${encode(`${title} ${url}`)}` },
  { label: 'Reddit', icon: 'bi-reddit', href: (url, title) => `https://www.reddit.com/submit?url=${encode(url)}&title=${encode(title)}` },
  { label: 'Hacker News', icon: 'bi-newspaper', href: (url, title) => `https://news.ycombinator.com/submitlink?u=${encode(url)}&t=${encode(title)}` },
  { label: 'Email', icon: 'bi-envelope', href: (url, title) => `mailto:?subject=${encode(title)}&body=${encode(url)}` },
  { label: 'WhatsApp', icon: 'bi-whatsapp', href: (url, title) => `https://wa.me/?text=${encode(`${title} ${url}`)}` },
  { label: 'Telegram', icon: 'bi-telegram', href: (url, title) => `https://t.me/share/url?url=${encode(url)}&text=${encode(title)}` },
];

export const AI_TARGETS: readonly AiTarget[] = [
  { label: 'Open in ChatGPT', icon: 'bi-chat-dots', href: prompt => `https://chatgpt.com/?q=${encode(prompt)}` },
  { label: 'Open in Claude', icon: 'bi-stars', href: prompt => `https://claude.ai/new?q=${encode(prompt)}` },
  { label: 'Open in GitHub Copilot', icon: 'bi-github', href: prompt => `https://github.com/copilot?prompt=${encode(prompt)}` },
];

export function aiPrompt(url: string, title: string): string {
  return `Read ${url} ("${title}" from the ResponseResultHandler docs) and help me apply it in my .NET project.`;
}
