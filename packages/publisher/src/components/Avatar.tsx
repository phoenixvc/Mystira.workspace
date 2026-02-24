import clsx from 'clsx';

export interface AvatarProps {
  src?: string;
  alt?: string;
  name?: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

function getInitials(name: string): string {
  return name
    .split(' ')
    .map(part => part[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

export function Avatar({ src, alt, name, size = 'md', className }: AvatarProps) {
  const initials = name ? getInitials(name) : '?';

  return (
    <div className={clsx('avatar', `avatar--${size}`, className)}>
      {src ? (
        <img src={src} alt={alt || name || 'Avatar'} className="avatar__image" />
      ) : (
        <span className="avatar__initials" aria-label={name || 'Unknown user'}>
          {initials}
        </span>
      )}
    </div>
  );
}
