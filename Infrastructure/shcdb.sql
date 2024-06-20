create table company
(
    id       int auto_increment
        primary key,
    name     varchar(255) null,
    location varchar(255) null,
    logo     varchar(255) null
)
    collate = utf8mb4_general_ci;

create table question_category
(
    id   int auto_increment
        primary key,
    name varchar(255) null
)
    collate = utf8mb4_general_ci;

create index idx_question_category_id
    on question_category (id);

create table squad
(
    id        int auto_increment
        primary key,
    name      varchar(255)                        null,
    createdAt timestamp default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP
)
    collate = utf8mb4_general_ci;

create table company_squad
(
    id         int auto_increment
        primary key,
    company_id int                                 null,
    squad_id   int                                 null,
    createdAt  timestamp default CURRENT_TIMESTAMP not null,
    constraint fk_company_squad_company
        foreign key (company_id) references company (id)
            on delete set null,
    constraint fk_company_squad_squad
        foreign key (squad_id) references squad (id)
            on delete set null
)
    collate = utf8mb4_general_ci;

create index company_id
    on company_squad (company_id);

create index squad_id
    on company_squad (squad_id);

create table user
(
    id           int auto_increment
        primary key,
    email        varchar(255)                                            not null,
    name         varchar(255)                                            null,
    passwordHash varchar(255)                                            null,
    role         enum ('Manager', 'Developer') default 'Developer'       null,
    createdAt    timestamp                     default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP
)
    collate = utf8mb4_general_ci;

create table healthcheck
(
    id          int auto_increment
        primary key,
    title       varchar(255)                        null,
    description text                                null,
    manager_id  int                                 null,
    createdAt   timestamp default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP,
    constraint fk_healthcheck_manager
        foreign key (manager_id) references user (id)
            on delete set null
)
    collate = utf8mb4_general_ci;

create index manager_id
    on healthcheck (manager_id);

create table healthcheck_version
(
    version_id     int auto_increment
        primary key,
    healthcheck_id int                                 null,
    version_number int                                 null,
    creation_date  timestamp default CURRENT_TIMESTAMP not null,
    notes          text                                null,
    isActive       tinyint   default 0                 null,
    constraint fk_healthcheck_version_healthcheck
        foreign key (healthcheck_id) references healthcheck (id)
            on delete set null
)
    collate = utf8mb4_general_ci;

create index healthcheck_id
    on healthcheck_version (healthcheck_id);

create table invitation_link
(
    id             int auto_increment
        primary key,
    healthcheck_id int                                  null,
    user_id        int                                  null,
    uniqueLink     varchar(255)                         null,
    isUsed         tinyint(1) default 0                 null,
    expiresAt      timestamp  default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP,
    version_id     int                                  null,
    constraint fk_invitation_link_healthcheck
        foreign key (healthcheck_id) references healthcheck (id)
            on delete cascade,
    constraint fk_invitation_link_user
        foreign key (user_id) references user (id)
            on delete cascade,
    constraint fk_invitation_link_version
        foreign key (version_id) references healthcheck_version (version_id)
            on delete cascade
)
    collate = utf8mb4_general_ci;

create index healthcheck_id
    on invitation_link (healthcheck_id);

create index user_id
    on invitation_link (user_id);

create index version_id
    on invitation_link (version_id);

create table question
(
    id             int auto_increment
        primary key,
    healthcheck_id int                                 null,
    category_id    int                                 null,
    priority       int       default 0                 null,
    description    text                                null,
    title          varchar(255)                        null,
    createdAt      timestamp default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP,
    version_id     int                                 null,
    constraint fk_question_category
        foreign key (category_id) references question_category (id)
            on delete set null,
    constraint fk_question_healthcheck
        foreign key (healthcheck_id) references healthcheck (id)
            on delete set null,
    constraint fk_question_version
        foreign key (version_id) references healthcheck_version (version_id)
            on delete cascade
)
    collate = utf8mb4_general_ci;

create table answer
(
    id          int auto_increment
        primary key,
    question_id int                                  null,
    user_id     int                                  null,
    answerColor enum ('Red', 'Yellow', 'Green')      null,
    comment     text                                 null,
    createdAt   timestamp  default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP,
    flagged     tinyint(1) default 0                 null,
    version_id  int                                  null,
    constraint fk_answer_question
        foreign key (question_id) references question (id)
            on delete set null,
    constraint fk_answer_user
        foreign key (user_id) references user (id)
            on delete cascade,
    constraint fk_answer_version
        foreign key (version_id) references healthcheck_version (version_id)
            on delete cascade
)
    collate = utf8mb4_general_ci;

create index question_id
    on answer (question_id);

create index user_id
    on answer (user_id);

create index version_id
    on answer (version_id);

create index healthcheck_id
    on question (healthcheck_id);

create index version_id
    on question (version_id);

create table squad_healthcheck
(
    id             int auto_increment
        primary key,
    squad_id       int null,
    healthcheck_id int null,
    constraint fk_squad_healthcheck_healthcheck
        foreign key (healthcheck_id) references healthcheck (id)
            on delete cascade,
    constraint fk_squad_healthcheck_squad
        foreign key (squad_id) references squad (id)
            on delete cascade
)
    collate = utf8mb4_general_ci;

create index healthcheck_id
    on squad_healthcheck (healthcheck_id);

create index squad_id
    on squad_healthcheck (squad_id);

create table squad_member
(
    id       int auto_increment
        primary key,
    user_id  int null,
    squad_id int null,
    constraint fk_squad_member_squad
        foreign key (squad_id) references squad (id)
            on delete cascade,
    constraint fk_squad_member_user
        foreign key (user_id) references user (id)
            on delete cascade
)
    collate = utf8mb4_general_ci;

create index squad_id
    on squad_member (squad_id);

create index user_id
    on squad_member (user_id);

create
    definer = shc@`%` procedure UpdateHealthCheckVersionStatus(IN input_version_id int)
BEGIN
    DECLARE v_invitations_count INT;

    SELECT COUNT(*) INTO v_invitations_count
    FROM invitation_link
    WHERE version_id = input_version_id;

    IF v_invitations_count > 0 THEN
        UPDATE healthcheck_version
        SET isActive = 1
        WHERE version_id = input_version_id;
    ELSE
        UPDATE healthcheck_version
        SET isActive = 0
        WHERE version_id = input_version_id;
    END IF;
END;


